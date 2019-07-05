using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Database;
using Protocol;

namespace Mineral.Core.Capsule
{
    public class DelegatedResourceCapsule : IProtoCapsule<DelegatedResource>
    {
        #region Field
        private DelegatedResource delegated_resource = null;
        #endregion


        #region Property
        public DelegatedResource Instance => this.delegated_resource;
        public byte[] Data => this.delegated_resource.ToByteArray();

        public ByteString From
        {
            get { return this.delegated_resource.From; }
        }

        public ByteString To
        {
            get { return this.delegated_resource.To; }
        }

        public long FrozenBalanceForEnergy
        {
            get { return this.delegated_resource.FrozenBalanceForEnergy; }
        }

        public long FrozenBalanceForBandwidth
        {
            get { return this.delegated_resource.FrozenBalanceForBandwidth; }
        }

        public long ExpireTimeForBandwidth
        {
            get { return this.delegated_resource.ExpireTimeForBandwidth; }
            set { this.delegated_resource.ExpireTimeForBandwidth = value; }
        }
        #endregion


        #region Contructor
        public DelegatedResourceCapsule(DelegatedResource delegated_resource)
        {
            this.delegated_resource = delegated_resource;
        }

        public DelegatedResourceCapsule(byte[] data)
        {
            try
            {
                this.delegated_resource = DelegatedResource.Parser.ParseFrom(data);
            }
            catch (InvalidProtocolBufferException e)
            {
                Logger.Debug(e.Message);
            }
        }

        public DelegatedResourceCapsule(ByteString from, ByteString to)
        {
            this.delegated_resource = new DelegatedResource();
            this.delegated_resource.From = from;
            this.delegated_resource.To = to;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public byte[] CreateDatabaseKey()
        {
            return CreateDatabaseKey(this.delegated_resource.From.ToByteArray(), this.delegated_resource.To.ToByteArray());
        }

        public static byte[] CreateDatabaseKey(byte[] from, byte[] to)
        {
            byte[] key = new byte[from.Length + to.Length];
            Array.Copy(from, 0, key, 0, from.Length);
            Array.Copy(to, 0, key, from.Length, to.Length);
            return key;
        }

        public void AddFrozenBalanceForEnergy(long energy, long expire_time)
        {
            this.delegated_resource = this.delegated_resource ?? new DelegatedResource();
            this.delegated_resource.FrozenBalanceForBandwidth = this.delegated_resource.FrozenBalanceForEnergy + energy;
            this.delegated_resource.ExpireTimeForEnergy = expire_time;
        }

        public void SetFrozenBalanceForEnergy(long energy, long expire_time)
        {
            this.delegated_resource = this.delegated_resource ?? new DelegatedResource();
            this.delegated_resource.FrozenBalanceForEnergy = energy;
            this.delegated_resource.ExpireTimeForEnergy = expire_time;
        }

        public void AddFrozenBalanceForBandwidth(long Bandwidth, long expire_time)
        {
            this.delegated_resource = this.delegated_resource ?? new DelegatedResource();
            this.delegated_resource.FrozenBalanceForBandwidth = this.delegated_resource.FrozenBalanceForBandwidth + Bandwidth;
            this.delegated_resource.ExpireTimeForBandwidth = expire_time;
        }

        public void SetFrozenBalanceForBandwidth(long Bandwidth, long expire_time)
        {
            this.delegated_resource = this.delegated_resource ?? new DelegatedResource();
            this.delegated_resource.FrozenBalanceForBandwidth = Bandwidth;
            this.delegated_resource.ExpireTimeForBandwidth = expire_time;
        }

        public long GetExpireTimeForEnergy(DataBaseManager manager)
        {
            long result = 0;
            if (manager.DynamicProperties.GetAllowMultiSign() == 0)
                result = this.delegated_resource.ExpireTimeForBandwidth;
            else
                result = this.delegated_resource.ExpireTimeForEnergy;

            return result;
        }

        public void SetExpireTimeForEnergy(long expire_time)
        {
            this.delegated_resource = this.delegated_resource ?? new DelegatedResource();
            this.delegated_resource.ExpireTimeForEnergy = expire_time;
        }
        #endregion
    }
}
