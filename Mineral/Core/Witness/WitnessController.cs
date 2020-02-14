using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Google.Protobuf;
using Mineral.Core.Capsule;
using Mineral.Core.Config;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;
using Mineral.Utils;

namespace Mineral.Core.Witness
{
    public class WitnessController
    {
        #region Field
        private DatabaseManager db_manager = null;
        private object locker = new object();
        private bool is_generating_block = false;
        #endregion


        #region Property
        public bool IsGeneratingBlock
        {
            get
            {
                return this.is_generating_block;
            }
            set
            {
                lock (locker)
                {
                    this.is_generating_block = value;
                }
            }
        }
        #endregion


        #region Contructor
        public WitnessController(DatabaseManager db_manager)
        {
            this.db_manager = db_manager;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void SortWitness(ref List<ByteString> item)
        {
            item = item.OrderBy(address => GetWitnessesByAddress(address), new WitnessSortComparer()).ToList();
        }

        private static bool WitnessSetChanged(List<ByteString> list1, List<ByteString> list2)
        {
            return !ArrayUtil.IsEqualCollection(list1, list2);
        }
        #endregion


        #region External Method
        public void InitializeWitness()
        {
            var it = this.db_manager.Block.GetEnumerator();
            bool reult = it.MoveNext();

            List<ByteString> witness_address = new List<ByteString>();
            this.db_manager.Witness.AllWitnesses.ForEach(witness =>
            {
                if (witness.IsJobs)
                {
                    witness_address.Add(witness.Address);
                }
            });
            
            SortWitness(ref witness_address);
            SetActiveWitnesses(witness_address);
            witness_address.ForEach(address => Logger.Info("InitializeWitness shuffled addresses : " + Wallet.AddressToBase58(address.ToByteArray())));
            SetCurrentShuffledWitnesses(witness_address);
        }

        public void AddWitness(ByteString address)
        {
            List<ByteString> active_witness = GetActiveWitnesses();
            active_witness.Add(address);
            SetActiveWitnesses(active_witness);
        }

        public WitnessCapsule GetWitnessesByAddress(ByteString address)
        {
            return this.db_manager.Witness.Get(address.ToByteArray());
        }

        public List<ByteString> GetActiveWitnesses()
        {
            return this.db_manager.WitnessSchedule.GetActiveWitnesses();
        }

        public void SetActiveWitnesses(List<ByteString> addresses)
        {
            this.db_manager.WitnessSchedule.SaveActiveWitnesses(addresses);
        }

        public List<ByteString> GetCurrentShuffledWitnesses()
        {
            return this.db_manager.WitnessSchedule.GetCurrentShuffledWitnesses();
        }

        public void SetCurrentShuffledWitnesses(List<ByteString> addresses)
        {
            this.db_manager.WitnessSchedule.SaveCurrentShuffledWitnesses(addresses);
        }

        private void PayStandbyWitness(List<ByteString> addresses)
        {
            long vote_sum = 0;
            long total_pay = this.db_manager.DynamicProperties.GetWitnessStandbyAllowance();
            foreach (ByteString address in addresses)
            {
                vote_sum += GetWitnessesByAddress(address).VoteCount;
            }

            if (vote_sum > 0)
            {
                foreach (ByteString address in addresses)
                {
                    long pay = (long)(GetWitnessesByAddress(address).VoteCount * ((double)total_pay / vote_sum));
                    AccountCapsule account = this.db_manager.Account.Get(address.ToByteArray());
                    account.Allowance = account.Allowance + pay;
                    this.db_manager.Account.Put(account.CreateDatabaseKey(), account);
                }
            }
        }

        public long GetSlotAtTime(long when)
        {
            long first_slot_time = GetSlotTime(1);
            if (when < first_slot_time)
                return 0;

            Logger.Debug(
                string.Format("NextFirstSlotTime:[{0}], when[{1}]",
                              first_slot_time.ToDateTime().ToLocalTime(),
                              when.ToDateTime().ToLocalTime()));

            return (when - first_slot_time) / Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL + 1;
        }

        public long GetAbsSlotAtTime(long when)
        {
            return (when - this.db_manager.GenesisBlock.Timestamp) / Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL;
        }

        public long GetSlotTime(long slot_num)
        {
            if (slot_num == 0)
                return Helper.CurrentTimeMillis();

            long interval = Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL;
            if (this.db_manager.DynamicProperties.GetLatestBlockHeaderNumber() == 0)
                return this.db_manager.GenesisBlock.Timestamp + slot_num * interval;

            if (this.db_manager.LastHeadBlockIsMaintenance())
                slot_num += this.db_manager.DynamicProperties.GetMaintenanceSkipSlots();

            long head_slot_time = this.db_manager.DynamicProperties.GetLatestBlockHeaderTimestamp();
            head_slot_time = head_slot_time - ((head_slot_time - this.db_manager.GenesisBlock.Timestamp) % interval);

            return head_slot_time + (interval * slot_num);
        }

        public long GetHeadSlot()
        {
            return (this.db_manager.DynamicProperties.GetLatestBlockHeaderTimestamp() - this.db_manager.GenesisBlock.Timestamp) / Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL;
        }

        public ByteString GetScheduleWitness(long slot)
        {
            long current_slot = GetHeadSlot() + slot;
            if (current_slot < 0)
            {
                throw new System.Exception("CurrentSlot should be positive");
            }

            int active_witness_count = GetActiveWitnesses().Count;
            int single_repeat = Parameter.ChainParameters.SINGLE_REPEAT;
            if (active_witness_count <= 0)
            {
                throw new System.Exception("Active Witnesses is null.");
            }

            int witness_Index = (int)current_slot % (active_witness_count * single_repeat);
            witness_Index /= single_repeat;

            Logger.Debug(
                string.Format("Current Slot : {0}, Witness Index : {1}, CrrentActiveWitness Size : {2}",
                              current_slot,
                              witness_Index,
                              active_witness_count));
                
            ByteString scheduled_witness = GetActiveWitnesses()?[witness_Index];
            Logger.Info(
                string.Format("Scheduled Witness : {0}, Current Slot : {1}",
                              Wallet.AddressToBase58(scheduled_witness.ToByteArray()),
                              current_slot));

            return scheduled_witness;
        }

        public bool ValidateWitnessSchedule(BlockCapsule block)
        {
            return ValidateWitnessSchedule(
                            block.Instance.BlockHeader.RawData.WitnessAddress,
                            block.Timestamp);
        }

        public bool ValidateWitnessSchedule(ByteString witness_address, long timestamp)
        {
            if (this.db_manager.DynamicProperties.GetLatestBlockHeaderNumber() == 0)
                return true;

            long abs_slot = GetAbsSlotAtTime(timestamp);
            long head_abs_slot = GetAbsSlotAtTime(this.db_manager.DynamicProperties.GetLatestBlockHeaderTimestamp());
            if (abs_slot <= head_abs_slot)
            {
                Logger.Warning("abs_slot is equals with head_abs_slot[" + abs_slot + "]");
                return false;
            }

            long slot = GetSlotAtTime(timestamp);
            ByteString scheduled_witness = GetScheduleWitness(slot);
            if (!scheduled_witness.Equals(witness_address))
            {
                Logger.Warning(
                    string.Format(
                        "Witness is out of order, scheduled Witness[{0}],BlockWitnessAddress[{1}], BlockTimeStamp[{2}], Slot[{3}]",
                        Wallet.AddressToBase58(scheduled_witness.ToByteArray()),
                        Wallet.AddressToBase58(witness_address.ToByteArray()),
                        timestamp.ToDateTime().ToLocalTime(),
                        slot));

                return false;
            }

            Logger.Debug(
                string.Format(
                    "Validate witness schedule successfully, scheduled witness:{0}",
                    Wallet.AddressToBase58(witness_address.ToByteArray())));

            return true;
        }

        public bool ActiveWitnessesContain(HashSet<ByteString> local_witness)
        {
            List<ByteString> active_witness = GetActiveWitnesses();
            foreach (ByteString witness_address in local_witness)
            {
                if (active_witness.Contains(witness_address))
                {
                    return true;
                }
            }

            return false;
        }

        public void TryRemovePowerOfGr()
        {
            if (this.db_manager.DynamicProperties.GetRemoveThePowerOfTheGr() == 1)
            {
                WitnessStore witness_store = this.db_manager.Witness;

                Args.Instance.GenesisBlock.Witnesses.ForEach(witness =>
                {
                    WitnessCapsule witness_capsule = witness_store.Get(witness.Address);
                    witness_capsule.VoteCount = witness_capsule.VoteCount - witness.VoteCount;

                    witness_store.Put(witness_capsule.CreateDatabaseKey(), witness_capsule);
                });

                this.db_manager.DynamicProperties.PutRemoveThePowerOfTheGr(-1);
            }
        }

        public Dictionary<ByteString, long> GetVoteCount(VotesStore votes_store)
        {
            Dictionary<ByteString, long> count_witness = new Dictionary<ByteString, long>();

            long count = 0;
            IEnumerator<KeyValuePair<byte[], VotesCapsule>> it = votes_store.GetEnumerator();
            while (it.MoveNext())
            {
                foreach (var vote in it.Current.Value.OldVotes)
                {
                    ByteString vote_address = vote.VoteAddress;
                    long vote_count = vote.VoteCount;

                    if (count_witness.ContainsKey(vote_address))
                    {
                        count_witness.TryGetValue(vote_address, out long value);
                        count_witness.Put(vote_address, value - vote_count);
                    }
                    else
                    {
                        count_witness.Put(vote_address, -vote_count);
                    }
                }

                foreach (var vote in it.Current.Value.NewVotes)
                {
                    ByteString vote_address = vote.VoteAddress;
                    long vote_count = vote.VoteCount;

                    if (count_witness.ContainsKey(vote_address))
                    {
                        count_witness.TryGetValue(vote_address, out long value);
                        count_witness.Put(vote_address, value + vote_count);
                    }
                    else
                    {
                        count_witness.Put(vote_address, vote_count);
                    }
                }

                count++;
                votes_store.Delete(it.Current.Key);
            }
            Logger.Info(string.Format(
                    "There is {0} new votes in this epoch",
                    count));

            return count_witness;
        }

        public void UpdateWitness()
        {
            TryRemovePowerOfGr();
            Dictionary<ByteString, long> count_witness = GetVoteCount(this.db_manager.Votes);

            if (count_witness.IsNullOrEmpty())
            {
                Logger.Info("No vote, no change to witness.");
            }
            else
            {
                List<ByteString> active_witness = GetActiveWitnesses();
                List<ByteString> witness_address = new List<ByteString>();
                this.db_manager.Witness.AllWitnesses.ForEach(witness =>
                {
                    witness_address.Add(witness.Address);
                });

                foreach (KeyValuePair<ByteString, long> pair in count_witness)
                {
                    WitnessCapsule witness = this.db_manager.Witness.Get(pair.Key.ToByteArray());
                    if (witness == null)
                    {
                        Logger.Warning(
                            string.Format("WitnessCapsule is null address is {0}", Wallet.AddressToBase58(pair.Key.ToByteArray())));

                        return;
                    }

                    AccountCapsule account = this.db_manager.Account.Get(pair.Key.ToByteArray());
                    if (account == null)
                    {
                        Logger.Warning("Witness account[" + Wallet.AddressToBase58(pair.Key.ToByteArray()) + "] not exists");
                    }
                    else
                    {
                        witness.VoteCount += pair.Value;
                        this.db_manager.Witness.Put(witness.CreateDatabaseKey(), witness);
                        Logger.Info(
                            string.Format(
                                "Address is {0}  ,count vote is {1}",
                                Wallet.AddressToBase58(witness.Address.ToByteArray()),
                                witness.VoteCount));
                    }
                }

                SortWitness(ref witness_address);
                if (witness_address.Count > Parameter.ChainParameters.MAX_ACTIVE_WITNESS_NUM)
                {
                    SetActiveWitnesses(witness_address.GetRange(0, Parameter.ChainParameters.MAX_ACTIVE_WITNESS_NUM));
                }
                else
                {
                    SetActiveWitnesses(witness_address);
                }

                if (witness_address.Count > Parameter.ChainParameters.WITNESS_STANDBY_LENGTH)
                {
                    PayStandbyWitness(witness_address.GetRange(0, Parameter.ChainParameters.WITNESS_STANDBY_LENGTH));
                }
                else
                {
                    PayStandbyWitness(witness_address);
                }

                List<ByteString> new_active_witness = GetActiveWitnesses();
                if (WitnessSetChanged(active_witness, new_active_witness))
                {
                    active_witness.ForEach(address =>
                    {
                        WitnessCapsule witness = GetWitnessesByAddress(address);
                        witness.IsJobs = false;
                        this.db_manager.Witness.Put(witness.CreateDatabaseKey(), witness);
                    });

                    new_active_witness.ForEach(address =>
                    {
                        WitnessCapsule witness = GetWitnessesByAddress(address);
                        witness.IsJobs = true;
                        this.db_manager.Witness.Put(witness.CreateDatabaseKey(), witness);
                    });
                }

                Logger.Info(
                    string.Format("Update Witness, Before:{0},\nAfter:{1}  ",
                                  string.Join(", ", active_witness.Select(x => Wallet.AddressToBase58(x.ToByteArray())).ToList()),
                                  string.Join(", ", new_active_witness.Select(x => Wallet.AddressToBase58(x.ToByteArray())).ToList()))
                );

            }
        }

        public int CalculateParticipationRate()
        {
            return this.db_manager.DynamicProperties.CalculateFilledSlotsCount();
        }

        public void DumpParticipationLog()
        {
            StringBuilder builder = new StringBuilder();
            int[] block_filled_slots = this.db_manager.DynamicProperties.GetBlockFilledSlots();

            builder.Append("Dump articipation log \n ")
                .Append("block filled slots : ")
                .Append(string.Join("", block_filled_slots))
                .Append(",")
                .Append("\n")
                .Append(" Head slot:")
                .Append(GetHeadSlot())
                .Append(",");

            List<ByteString> active_witness = GetActiveWitnesses();
            active_witness.ForEach(active =>
            {
                WitnessCapsule witness = this.db_manager.Witness.Get(active.ToByteArray());
                builder.Append("\n")
                    .Append(" Witness : ")
                    .Append(witness.ToHexString())
                    .Append(",")
                    .Append("LatestBlockNum : ")
                    .Append(witness.LatestBlockNum)
                    .Append(",")
                    .Append("LatestSlotNum : ")
                    .Append(witness.LatestSlotNum)
                    .Append(".");
            });
            Logger.Debug(builder.ToString());
        }
        #endregion
    }
}
