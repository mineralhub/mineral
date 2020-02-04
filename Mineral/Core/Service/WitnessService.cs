using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Google.Protobuf;
using Mineral.Common.Application;
using Mineral.Common.Backup;
using Mineral.Core.Capsule;
using Mineral.Core.Config;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;
using Mineral.Core.Net;
using Mineral.Core.Net.Messages;
using Mineral.Core.Witness;
using Mineral.Cryptography;
using Mineral.Utils;

namespace Mineral.Core.Service
{
    public class WitnessService : IService
    {
        #region Field
        private static readonly int MIN_PARTICIPATION_RATE = Args.Instance.Node.MinParticipationRate;
        private static readonly int PRODUCE_TIME_OUT = 500;
        private static volatile bool need_sync_check = (bool)Args.Instance.Block.NeedSyncCheck;

        private Thread thread_generate = null;
        private readonly DatabaseManager db_manager = null;
        private readonly BackupManager backup_manager = null;
        private readonly MineralNetService net_service = null;
        private readonly BackupServer backup_server = null;

        private Dictionary<ByteString, byte[]> privatekeys = new Dictionary<ByteString, byte[]>();
        private Dictionary<byte[], byte[]> privatekey_addresses = new Dictionary<byte[], byte[]>(new ByteArrayEqualComparer());
        protected Dictionary<ByteString, WitnessCapsule> local_witness_states = new Dictionary<ByteString, WitnessCapsule>();

        private WitnessController controller;
        private volatile bool is_running = false;
        private int block_count = 0;
        private long block_time = 0;
        private long block_cycle = Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL * Parameter.ChainParameters.MAX_ACTIVE_WITNESS_NUM;


        #endregion


        #region Property
        public static bool IsNeedSyncCheck
        {
            get { return need_sync_check; }
        }
        #endregion


        #region Constructor
        public WitnessService(Manager manager)
        {
            this.db_manager = manager.DBManager;
            this.backup_manager = manager.BackupManager;
            this.net_service = manager.NetService;
            this.backup_server = manager.BackupServer;

            this.controller = this.db_manager.WitnessController;
            this.db_manager.WitnessService = this;
            this.thread_generate = new Thread(new ThreadStart(ScheduleProductionLoop));

            new Thread(new ThreadStart(() =>
            {
                while (need_sync_check)
                {
                    Thread.Sleep(100);
                }

                this.backup_server.InitServer();
            })).Start();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void ScheduleProductionLoop()
        {
            if (this.local_witness_states == null
                || this.local_witness_states.Keys == null
                || this.local_witness_states.Keys.Count == 0)
            {
                Logger.Error("LocalWitness is null.");
                return;
            }

            while (this.is_running)
            {
                try
                {
                    if (need_sync_check)
                    {
                        Thread.Sleep(500);
                    }
                    else
                    {
                        DateTime time = DateTime.Now;
                        long next_second = Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL
                            - (time.Second * 1000 + time.Millisecond) % Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL;

                        if (next_second < 50)
                        {
                            next_second = next_second + Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL;
                        }

                        DateTime time_next = time.AddSeconds(next_second);
                        Logger.Debug(
                            "ProductionLoop sleep : " + next_second + " ms,next time:" + time_next);

                        Thread.Sleep((int)next_second);
                    }

                    BlockProductionLoop();
                }
                catch (System.Exception e)
                {
                    Logger.Error("Unknown throwable happened in witness loop" + e.Message);
                }
            }
        }

        private void BlockProductionLoop()
        {
            BlockProductionCondition result = TryProduceBlock();

            if (result <= BlockProductionCondition.NOT_MY_TURN)
            {
                Logger.Debug(result.ToString());
            }
            else
            {
                Logger.Info(result.ToString());
            }
        }

        private BlockProductionCondition TryProduceBlock()
        {
            Logger.Info("Try Produce Block");

            long now = Helper.CurrentTimeMillis() + 50;
            if (need_sync_check)
            {
                long next_slot_time = this.controller.GetSlotTime(1);
                if (next_slot_time > now)
                {
                    need_sync_check = false;
                    Thread.Sleep((int)(next_slot_time - now));
                    now = Helper.CurrentTimeMillis();
                }
                else
                {
                    Logger.Debug(
                        string.Format("Not sync, Now : {0}, HeadBlockTime:{1}, HeadBlockNumber : {2}, HeadBlockId:{3}",
                                      now.ToDateTime().ToLocalTime(),
                                      this.db_manager.DynamicProperties.GetLatestBlockHeaderTimestamp().ToDateTime().ToLocalTime(),
                                      this.db_manager.DynamicProperties.GetLatestBlockHeaderNumber(),
                                      this.db_manager.DynamicProperties.GetLatestBlockHeaderHash()));

                    return BlockProductionCondition.NOT_SYNCED;
                }
            }

            if (this.backup_manager.Status != BackupManager.BackupStatus.MASTER)
            {
                return BlockProductionCondition.BACKUP_STATUS_IS_NOT_MASTER;
            }

            if (DupWitnessCheck())
            {
                return BlockProductionCondition.DUP_WITNESS;
            }

            int participation = this.controller.CalculateParticipationRate();
            if (participation < MIN_PARTICIPATION_RATE)
            {
                Logger.Warning(
                    string.Format("Participation[{0}] > MIN_PARTICIPATION_RATE[{1}]",
                                  participation,
                                  MIN_PARTICIPATION_RATE));

                this.controller.DumpParticipationLog();

                return BlockProductionCondition.LOW_PARTICIPATION;
            }

            if (!this.controller.ActiveWitnessesContain(this.local_witness_states.Keys.ToHashSet()))
            {
                string log_address = "";
                foreach (var addr in this.controller.GetActiveWitnesses().Select(witness => Wallet.AddressToBase58(witness.ToByteArray())))
                {
                    log_address += "\n" + "[" + addr + "]";
                }

                Logger.Info(
                    string.Format("Unelected. Elected Witnesses: {0}", log_address));
                                  
                return BlockProductionCondition.UNELECTED;
            }

            try
            {
                BlockCapsule block = null;

                //lock (this.db_manager)
                {
                    long slot = this.controller.GetSlotAtTime(now);
                    Logger.Debug("Slot : " + slot);

                    if (slot == 0)
                    {
                        Logger.Info(
                            string.Format("Not time yet, Now : {0}, HeadBlockTime : {1}, HeadBlockNumber : {2}, HeadBlockId:{3}",
                                          now.ToDateTime().ToLocalTime(),
                                          this.db_manager.DynamicProperties.GetLatestBlockHeaderTimestamp().ToDateTime().ToLocalTime(),
                                          this.db_manager.DynamicProperties.GetLatestBlockHeaderNumber(),
                                          this.db_manager.DynamicProperties.GetLatestBlockHeaderHash()));

                        return BlockProductionCondition.NOT_TIME_YET;
                    }

                    if (now < this.db_manager.DynamicProperties.GetLatestBlockHeaderTimestamp())
                    {
                        Logger.Warning(
                            string.Format("timestamp : {0} less than or equal to the previous block timestamp : {1}",
                                          now.ToDateTime().ToLocalTime(),
                                          this.db_manager.DynamicProperties.GetLatestBlockHeaderTimestamp().ToDateTime().ToLocalTime()));

                        return BlockProductionCondition.EXCEPTION_PRODUCING_BLOCK;
                    }

                    ByteString scheduled_witness = this.controller.GetScheduleWitness(slot);
                    if (!this.local_witness_states.ContainsKey(scheduled_witness))
                    {
                        Logger.Info(
                            string.Format("It's not my turn, ScheduledWitness[{0}], Slot[{1}], AbsSlot[{2}],",
                                          Wallet.AddressToBase58(scheduled_witness.ToByteArray()),
                                          slot,
                                          controller.GetAbsSlotAtTime(now)));

                        return BlockProductionCondition.NOT_MY_TURN;
                    }

                    long scheduled_time = controller.GetSlotTime(slot);
                    if (scheduled_time - now > PRODUCE_TIME_OUT)
                    {
                        return BlockProductionCondition.LAG;
                    }

                    if (!this.privatekeys.ContainsKey(scheduled_witness))
                    {
                        return BlockProductionCondition.NO_PRIVATE_KEY;
                    }

                    this.controller.IsGeneratingBlock = true;
                    block = GenerateBlock(scheduled_time, scheduled_witness, this.db_manager.LastHeadBlockIsMaintenance());

                    if (block == null)
                    {
                        Logger.Warning("Exception when generate block");
                        return BlockProductionCondition.EXCEPTION_PRODUCING_BLOCK;
                    }

                    int block_produce_timeout = Args.Instance.Node.BlockProducedTimeout;

                    long timeout = Math.Min(Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL * block_produce_timeout / 100 + 500,
                                            Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL);

                    if (DateTime.Now.Millisecond - now > timeout)
                    {
                        Logger.Warning(
                            string.Format("Task timeout ( > {0}ms)，startTime:{1}, endTime:{2}",
                                          timeout,
                                          now.ToDateTime().ToLocalTime(),
                                          DateTime.Now));

                        this.db_manager.EraseBlock();
                        return BlockProductionCondition.TIME_OUT;
                    }
                }

                Logger.Info(
                    string.Format(
                        "Produce block successfully, BlockNumber:{0}, AbsSlot[{1}], BlockId:{2}, TransactionSize:{3}, BlockTime:{4}, ParentBlockId:{5}",
                        block.Num,
                        controller.GetAbsSlotAtTime(now),
                        block.Id,
                        block.Transactions.Count,
                        block.Timestamp.ToDateTime().ToLocalTime(),
                        block.ParentId));

                Logger.Refactoring(
                    string.Format("Produce block successfully, block number {0}", block.Num));

                ThreadPool.GetAvailableThreads(out int worker, out int io);
                Logger.Refactoring(string.Format("ThreadPool available count worker {0}, io {1}", worker, io));

                BroadcastBlock(block);

                return BlockProductionCondition.PRODUCED;
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
                return BlockProductionCondition.EXCEPTION_PRODUCING_BLOCK;
            }
            finally
            {
                this.controller.IsGeneratingBlock = false;
            }
        }

        private bool DupWitnessCheck()
        {
            if (this.block_count == 0)
            {
                return false;
            }

            if (Helper.CurrentTimeMillis() - Interlocked.Read(ref this.block_time) > this.block_count * this.block_cycle)
            {
                Interlocked.Exchange(ref this.block_count, 0);

                return false;
            }

            return true;
        }

        private BlockCapsule GenerateBlock(long when, ByteString witness_address, bool is_mainternance)
        {
            this.local_witness_states.TryGetValue(witness_address, out WitnessCapsule witness);
            this.privatekeys.TryGetValue(witness_address, out byte[] privatekey);

            return this.db_manager.GenerateBlock(witness, when, privatekey, is_mainternance, true);
        }

        private void BroadcastBlock(BlockCapsule block)
        {
            try
            {
                this.net_service.Broadcast(new BlockMessage(block.Data));
            }
            catch (System.Exception)
            {
                throw new System.Exception("BroadcastBlock error");
            }
        }
        #endregion


        #region External Method
        public void Init()
        {
            if (Args.Instance.LocalWitness.GetPrivateKey().IsNullOrEmpty())
                return;

            byte[] privatekey = Args.Instance.LocalWitness.GetPrivateKey();
            byte[] witness_address = Args.Instance.LocalWitness.GetWitnessAccountAddress();
            byte[] privatekey_address = Wallet.PublickKeyToAddress(ECKey.FromPrivateKey(privatekey).PublicKey);

            WitnessCapsule witness = Manager.Instance.DBManager.Witness.Get(witness_address);
            if (witness == null)
            {
                Logger.Warning(
                    string.Format("WitnessCapsule[{0}] is not in witnessStore",
                                  witness_address));

                witness = new WitnessCapsule(ByteString.CopyFrom(witness_address));
            }

            this.privatekeys.Add(witness.Address, privatekey);
            this.local_witness_states.Add(witness.Address, witness);
            this.privatekey_addresses.Add(privatekey, privatekey_address);
        }

        public void Init(Args args)
        {
            Init();
        }

        public void Start()
        {
            this.is_running = true;
            this.thread_generate.Start();
        }

        public void Stop()
        {
            this.is_running = false;
            this.thread_generate.Interrupt();
        }

        public bool ValidateWitnessPermission(ByteString scheduled_witness)
        {
            if (this.db_manager.DynamicProperties.GetAllowMultiSign() == 1)
            {
                this.privatekeys.TryGetValue(scheduled_witness, out byte[] privatekey);
                this.privatekey_addresses.TryGetValue(privatekey, out byte[] permission_address);

                AccountCapsule witnessAccount = this.db_manager.Account.Get(scheduled_witness.ToByteArray());

                if (!permission_address.SequenceEqual(witnessAccount.GetWitnessPermissionAddress()))
                {
                    return false;
                }
            }

            return true;
        }

        public void CheckDupWitness(BlockCapsule block)
        {
            if (block.IsGenerateMyself)
                return;

            if (need_sync_check)
                return;

            if (Helper.CurrentTimeMillis() - block.Timestamp > Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL)
                return;

            if (!this.privatekeys.ContainsKey(block.WitnessAddress))
                return;

            if (this.backup_manager.Status != BackupManager.BackupStatus.MASTER)
                return;

            if (this.block_count == 0)
                Interlocked.Exchange(ref this.block_count, new Random().Next(10));
            else
                Interlocked.Exchange(ref this.block_count, 10);

            Interlocked.Exchange(ref this.block_time, Helper.CurrentTimeMillis());

            Logger.Warning("Duplicate block produced : " + block.ToString());
        }
        #endregion
    }
}
