using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Google.Protobuf;
using Mineral.Core;
using Mineral.Core.Capsule;
using Mineral.Core.Config;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;
using static Mineral.Core.Config.Parameter;

namespace Mineral.Common.Utils
{
    public class ForkController
    {
        #region Field
        private static readonly byte VERSION_DOWNGRADE = (byte)0;
        private static readonly byte VERSION_UPGRADE = (byte)1;
        private static readonly byte[] check = null;

        private static ForkController instance = null;
        private DatabaseManager db_manager;
        #endregion


        #region Property
        public static ForkController Instance
        {
            get
            {
                if (instance == null)
                    instance = new ForkController();

                return instance;
            }
        }

        public DatabaseManager DBManager => this.db_manager;
        #endregion


        #region Contructor
        static ForkController()
        {
            check = new byte[1024];
            check = Enumerable.Repeat(VERSION_UPGRADE, check.Length).ToArray();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private bool Check(byte[] stats)
        {
            if (stats == null || stats.Length == 0)
                return false;

            for (int i = 0; i < stats.Length; i++)
            {
                if (check[i] != stats[i])
                    return false;
            }

            return true;
        }

        private bool CheckForEnergyLimit()
        {
            long block_num = this.db_manager.DynamicProperties.GetLatestBlockHeaderNumber();
            return block_num >= Args.Instance.BlockNumEnergyLimit;
        }

        private void Downgrade(int version, int slot)
        {
            foreach (ForkBlockVersion v in Enum.GetValues(typeof(ForkBlockVersion)))
            {
                if ((int)v > version)
                {
                    byte[] stats = this.db_manager.DynamicProperties.StatsByVersion((int)v);
                    if (stats != null && !Check(stats))
                    {
                        stats[slot] = VERSION_DOWNGRADE;
                        this.db_manager.DynamicProperties.StatsByVersion((int)v, stats);
                    }
                }
            }
        }

        private void Upgrade(int version, int slot_size)
        {
            foreach (ForkBlockVersion v in Enum.GetValues(typeof(ForkBlockVersion)))
            {
                if ((int)v < version)
                {
                    byte[] stats = this.db_manager.DynamicProperties.StatsByVersion((int)v);
                    if (!Check(stats))
                    {
                        if (stats == null || stats.Length == 0)
                        {
                            stats = new byte[slot_size];
                        }
                        stats = Enumerable.Repeat(VERSION_UPGRADE, stats.Length).ToArray();
                        this.db_manager.DynamicProperties.StatsByVersion((int)v, stats);
                    }
                }
            }
        }
        #endregion


        #region External Method
        public void Init(DatabaseManager db_manager)
        {
            this.db_manager = db_manager;
        }

        public bool Pass(ForkBlockVersion version)
        {
            return Pass((int)version);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Pass(int version)
        {
            if (version == (int)ForkBlockVersion.ENERGY_LIMIT)
            {
                return CheckForEnergyLimit();
            }
            
            byte[] stats = this.db_manager.DynamicProperties.StatsByVersion(version);
            return Check(stats);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Update(BlockCapsule block)
        {
            List<ByteString> witnesses = this.db_manager.WitnessController.GetActiveWitnesses();
            ByteString witness = block.WitnessAddress;
            int slot = witnesses.IndexOf(witness);
            if (slot < 0)
                return;

            int version = block.Instance.BlockHeader.RawData.Version;
            if (version < Parameter.ForkBlockVersionParameters.ENERGY_LIMIT)
                return;

            Downgrade(version, slot);

            byte[] stats = this.db_manager.DynamicProperties.StatsByVersion(version);
            if (Check(stats))
            {
                Upgrade(version, stats.Length);
                return;
            }

            if (stats == null || stats.Length != witnesses.Count)
            {
                stats = new byte[witnesses.Count];
            }

            stats[slot] = VERSION_UPGRADE;
            this.db_manager.DynamicProperties.StatsByVersion(version, stats);
            Logger.Info(
                string.Format(
                    "*******update hard fork:{0}, witness size:{1}, solt:{2}, witness:{3}, version:{4}",

                    Enumerable.Zip<ByteString, byte, KeyValuePair<ByteString, byte>>(
                                    witnesses,
                                    stats,
                                    (ByteString key, byte value) =>
                                    {
                                        return new KeyValuePair<ByteString, byte>(key, value);
                                    })
                                .Select(pair =>
                                {
                                    string address = Wallet.Encode58Check(pair.Key.ToByteArray());
                                    address = address.Substring(address.Length - 4);
                                    return new KeyValuePair<string, byte>(address, pair.Value);
                                })
                                .ToList()
                                .ToString(),
                    witnesses.Count,
                    slot,
                    Wallet.Encode58Check(witness.ToByteArray()),
                    version));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Reset()
        {
            foreach (ForkBlockVersion v in Enum.GetValues(typeof(ForkBlockVersion)))
            {
                byte[] stats = this.db_manager.DynamicProperties.StatsByVersion((int)v);
                if (stats != null && !Check(stats))
                {
                    stats = Enumerable.Repeat(VERSION_DOWNGRADE, stats.Length).ToArray();
                    this.db_manager.DynamicProperties.StatsByVersion((int)v, stats);
                }
            }
        }
        #endregion
    }
}
