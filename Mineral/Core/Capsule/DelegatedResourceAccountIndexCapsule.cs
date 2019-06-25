using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Protocol;

namespace Mineral.Core.Capsule
{
    public class DelegatedResourceAccountIndexCapsule : IProtoCapsule<DelegatedResourceAccountIndex>
    {
        #region Field
        private DelegatedResourceAccountIndex instance = null;
        #endregion


        #region Property
        public DelegatedResourceAccountIndex Instance => this.instance;
        public byte[] Data => this.instance.ToByteArray();

        public ByteString Account
        {
            get { return this.instance.Account; }
            set { this.instance.Account = value; }
        }

        public List<ByteString> FromAccounts
        {
            get { return new List<ByteString>(this.instance.FromAccounts); }
            set
            {
                this.instance.FromAccounts.Clear();
                this.instance.FromAccounts.AddRange(value);
            }
        }

        public List<ByteString> ToAccounts
        {
            get { return new List<ByteString>(this.instance.ToAccounts); }
            set
            {
                this.instance.ToAccounts.Clear();
                this.instance.ToAccounts.AddRange(value);
            }
        }
        #endregion


        #region Contructor
        public DelegatedResourceAccountIndexCapsule(DelegatedResourceAccountIndex instance)
        {
            this.instance = instance;
        }

        public DelegatedResourceAccountIndexCapsule(byte[] data)
        {
            try
            {
                this.instance = DelegatedResourceAccountIndex.Parser.ParseFrom(data);
            }
            catch (InvalidProtocolBufferException e)
            {
                Logger.Debug(e.Message);
            }
        }

        public DelegatedResourceAccountIndexCapsule(ByteString address)
        {
            this.instance = new DelegatedResourceAccountIndex();
            this.instance.Account = address;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public byte[] CreateDatabaseKey()
        {
            return this.instance.Account.ToByteArray();
        }

        public void AddFromAccount(ByteString account)
        {
            this.instance.FromAccounts.Add(account);
        }

        public void AddToAccount(ByteString account)
        {
            this.instance.FromAccounts.Add(account);
        }
        #endregion
    }
}
