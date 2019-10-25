using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Protocol;

namespace Mineral.Core.Capsule
{
    public class VotesCapsule : IProtoCapsule<Votes>
    {
        #region Field
        private Votes votes = null;
        #endregion


        #region Property
        public Votes Instance => this.votes;
        public byte[] Data => this.votes.ToByteArray();

        public ByteString Address
        {
            get { return this.votes.Address; }
            set { this.votes.Address = value; }
        }

        public List<Vote> NewVotes
        {
            get { return new List<Vote>(this.votes.NewVotes); }
            set { this.votes.NewVotes.Clear(); this.votes.NewVotes.AddRange(value); }
        }

        public List<Vote> OldVotes
        {
            get { return new List<Vote>(this.votes.OldVotes); }
            set { this.votes.OldVotes.Clear(); this.votes.OldVotes.AddRange(value); }
        }
        #endregion


        #region Contructor
        public VotesCapsule(ByteString address, List<Vote> old_votes)
        {
            this.votes = new Votes();
            this.votes.Address = address;
            this.votes.OldVotes.AddRange(old_votes);
        }

        public VotesCapsule(byte[] data)
        {
            try
            {
                this.votes = Votes.Parser.ParseFrom(data);
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
        public void AddNewVotes(ByteString vote_address, long vote_count)
        {
            this.votes.NewVotes.Add(new Vote() { VoteAddress = vote_address, VoteCount = vote_count });
        }

        public void ClearNewVotes()
        {
            this.votes.NewVotes.Clear();
        }

        public byte[] CreateDatabaseKey()
        {
            return this.votes.Address.ToByteArray();
        }

        public string ToHexString()
        {
            return this.votes.Address.ToByteArray().ToHexString();
        }
        #endregion
    }
}
