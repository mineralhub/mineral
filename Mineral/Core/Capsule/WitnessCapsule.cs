using System;
using Google.Protobuf;
using Protocol;

namespace Mineral.Core.Capsule
{
    using Witness = Protocol.Witness;

    public class WitnessCapsule : IProtoCapsule<Witness>, IComparable<WitnessCapsule>
    {
        #region Field
        private Witness witness = null;
        #endregion


        #region Property
        public Witness Instance => this.witness;
        public byte[] Data => this.witness.ToByteArray();

        public ByteString Address
        {
            get { return this.witness.Address; }
        }

        public long VoteCount
        {
            get { return this.witness.VoteCount; }
            set { this.witness.VoteCount = value; }
        }

        public long TotalProduced
        {
            get { return this.witness.TotalProduced; }
            set { this.witness.TotalProduced = value; }
        }

        public long TotalMissed
        {
            get { return this.witness.TotalMissed; }
            set { this.witness.TotalMissed = value; }
        }

        public long LatestBlockNum
        {
            get { return this.witness.LatestBlockNum; }
            set { this.witness.LatestBlockNum = value; }
        }

        public long LatestSlotNum
        {
            get { return this.witness.LatestSlotNum; }
            set { this.witness.LatestSlotNum = value; }
        }

        public ByteString PublicKey
        {
            get { return this.witness.PubKey; }
            set { this.witness.PubKey = value; }
        }

        public bool IsJobs
        {
            get { return this.witness.IsJobs; }
            set { this.witness.IsJobs = value; }
        }

        public string Url
        {
            get { return this.witness.Url; }
            set { this.witness.Url = value; }
        }
        #endregion


        #region Contructor
        public WitnessCapsule(Witness witness)
        {
            this.witness = witness;
        }

        public WitnessCapsule(ByteString address)
        {
            this.witness = new Witness();
            this.witness.Address = address;
        }

        public WitnessCapsule(ByteString address, long vote_count, string url)
        {
            this.witness = new Witness();
            this.witness.Address = address;
            this.witness.VoteCount = vote_count;
            this.witness.Url = url;
        }

        public WitnessCapsule(byte[] data)
        {
            try
            {
                this.witness = Witness.Parser.ParseFrom(data);
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public byte[] CreateDatabaseKey()
        {
            return this.witness.Address.ToByteArray();
        }

        public string CreateReadableString()
        {
            return this.witness.Address.ToByteArray().ToHexString();
        }

        public int CompareTo(WitnessCapsule other)
        {
            return other.vote
        }
        #endregion
    }
}
