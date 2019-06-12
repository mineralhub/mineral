using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Protocol;

namespace Mineral.Core.Database.Fast
{
    public class AccountStateEntity
    {
        #region Field
        private Account account;
        #endregion


        #region Property
        public Account Account
        {
            get { return this.account; }
            set { this.account = value; }
        }
        #endregion


        #region Contructor
        public AccountStateEntity() { }
        public AccountStateEntity(Account account)
        {
            this.account = new Account(account);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public byte[] ToByteArray()
        {
            return this.account.ToByteArray();
        }

        public static AccountStateEntity Parse(byte[] data)
        {
            AccountStateEntity result = null;
            try
            {
                result = new AccountStateEntity(Account.Parser.ParseFrom(data));
            }
            catch (System.Exception e)
            {
                Logger.Error("parse to AccountStateEntity error, " + e.Message);
            }

            return result;
        }

        public override string ToString()
        {
            return "Address" + Wallet.Encode58Check(this.account?.Address.ToByteArray()) + "; " + this.account?.ToString();
        }
        #endregion
    }
}
