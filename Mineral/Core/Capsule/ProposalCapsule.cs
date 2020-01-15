using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Config;
using Protocol;
using static Protocol.Proposal.Types;

namespace Mineral.Core.Capsule
{
    public class ProposalCapsule : IProtoCapsule<Proposal>
    {
        #region Field
        private Proposal proposal = null;
        #endregion


        #region Property
        public Proposal Instance => this.proposal;
        public byte[] Data => this.proposal.ToByteArray();

        public long Id
        {
            get { return this.proposal.ProposalId; }
            set { this.proposal.ProposalId = value; }
        }

        public ByteString Address
        {
            get { return this.proposal.ProposerAddress; }
            set { this.proposal.ProposerAddress = value; }
        }

        public Dictionary<long, long> Parameters
        {
            get { return new Dictionary<long, long>(this.proposal.Parameters); }
            set { this.proposal.Parameters.Add(value); }
        }

        public long ExpirationTime
        {
            get { return this.proposal.ExpirationTime; }
            set { this.proposal.ExpirationTime = value; }
        }

        public long CreateTime
        {
            get { return this.proposal.CreateTime; }
            set { this.proposal.CreateTime = value; }
        }

        public State State
        {
            get { return this.proposal.State; }
            set { this.proposal.State = value; }
        }

        public IList<ByteString> Approvals
        {
            get { return this.proposal.Approvals; }
        }

        public bool HasProcessed
        {
            get { return this.proposal.State == State.Disapproved || this.proposal.State == State.Approved; }
        }

        public bool HasCanceled
        {
            get { return this.proposal.State == State.Canceled; }
        }
        #endregion


        #region Contructor
        public ProposalCapsule(Proposal proposal)
        {
            this.proposal = proposal;
        }

        public ProposalCapsule(ByteString address, long id)
        {
            this.proposal = new Proposal();
            this.proposal.ProposerAddress = address;
            this.proposal.ProposalId = id;
        }

        public ProposalCapsule(byte[] data)
        {
            try
            {
                this.proposal = Proposal.Parser.ParseFrom(data);
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
            return CalculateDatabaseKey(this.proposal.ProposalId);
        }

        public static byte[] CalculateDatabaseKey(long number)
        {
            return BitConverter.GetBytes(number);
        }

        public bool HasExpired(long time)
        {
            return this.proposal.ExpirationTime <= time;
        }

        public bool HasMostApprovals(List<ByteString> active_witness)
        {
            List<ByteString> contains = new List<ByteString>(this.proposal.Approvals.Where(witness => active_witness.Contains(witness)));
            long count = contains.Count;

            if (count != this.proposal.Approvals.Count())
            {
                List<ByteString> not_contains = new List<ByteString>(this.proposal.Approvals.Where(witness => !active_witness.Contains(witness)));

                List<string> addresses = not_contains.Select(witness => Wallet.AddressToBase58(witness.ToByteArray())).ToList();
                Logger.Info("Invalid approval list : " + addresses.ToString());
            }

            if (active_witness.Count != Parameter.ChainParameters.MAX_ACTIVE_WITNESS_NUM)
            {
                Logger.Info("Active witness count = " + active_witness.Count());
            }

            return count >= active_witness.Count * 7 / 10;
        }
        #endregion
    }
}
