using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Google.Protobuf;
using Mineral.Core.Capsule.Util;
using Mineral.Core.Database;
using Protocol;

namespace Mineral.Core.Capsule
{
    public class ExchangeCapsule : IProtoCapsule<Exchange>
    {
        #region Field
        private readonly byte[] COMPARE_CHARICTOR = Encoding.UTF8.GetBytes("_");
        private Exchange instance = null;
        #endregion


        #region Property
        public Exchange Instance => this.instance;
        public byte[] Data => this.instance.ToByteArray();

        public long Id
        {
            get { return this.instance.ExchangeId; }
            set { this.instance.ExchangeId = value; }
        }

        public ByteString CreatorAddress
        {
            get { return this.instance.CreatorAddress; }
            set { this.instance.CreatorAddress = value; }
        }

        public long CreateTime
        {
            get { return this.instance.CreateTime; }
            set { this.instance.CreateTime = value; }
        }

        public ByteString FirstTokenId
        {
            get { return this.instance.FirstTokenId; }
            set { this.instance.FirstTokenId = value; }
        }

        public ByteString SecondTokenId
        {
            get { return this.instance.SecondTokenId; }
            set { this.instance.SecondTokenId = value; }
        }

        public long FirstTokenBalance
        {
            get { return this.instance.FirstTokenBalance; }
        }

        public long SecondTokenBalance
        {
            get { return this.instance.SecondTokenBalance; }
        }
        #endregion


        #region Contructor
        public ExchangeCapsule(Exchange instance)
        {
            this.instance = instance;
        }

        public ExchangeCapsule(byte[] data)
        {
            try
            {
                this.instance = Exchange.Parser.ParseFrom(data);
            }
            catch (InvalidProtocolBufferException e)
            {
                Logger.Debug(e.Message);
            }
        }

        public ExchangeCapsule(ByteString address, long id, long create_time, byte[] first_token_id, byte[] second_token_id)
        {
            this.instance = new Exchange();
            this.instance.ExchangeId = id;
            this.instance.CreatorAddress = address;
            this.instance.CreateTime = create_time;
            this.instance.FirstTokenId = ByteString.CopyFrom(first_token_id);
            this.instance.SecondTokenId = ByteString.CopyFrom(second_token_id);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public byte[] CreateDatabaseKey()
        {
            return CalculateDatabaseKey(Id);
        }

        public static byte[] CalculateDatabaseKey(long number)
        {
            return BitConverter.GetBytes(number);
        }

        public void SetBalance(long first_token_balance, long second_token_balance)
        {
            this.instance = this.instance ?? new Exchange();
            this.instance.FirstTokenBalance = first_token_balance;
            this.instance.SecondTokenBalance = second_token_balance;
        }

        public long Transaction(byte[] sell_token_id, long sell_quantity)
        {
            long supply = 1_000_000_000_000_000_000L;
            ExchangeProcessor processor = new ExchangeProcessor(supply);

            long buy_quantity = 0;
            long first_balance = this.instance.FirstTokenBalance;
            long second_balance = this.instance.SecondTokenBalance;

            if (this.instance.FirstTokenId.Equals(ByteString.CopyFrom(sell_token_id)))
            {
                buy_quantity = processor.Exchange(first_balance,
                                                  second_balance,
                                                  sell_quantity);

                this.instance = this.instance ?? new Exchange();
                this.instance.FirstTokenBalance = first_balance + sell_quantity;
                this.instance.SecondTokenBalance = second_balance - buy_quantity;
            }
            else
            {
                buy_quantity = processor.Exchange(second_balance,
                                                  first_balance,
                                                  sell_quantity);

                this.instance = this.instance ?? new Exchange();
                this.instance.FirstTokenBalance = first_balance - buy_quantity;
                this.instance.SecondTokenBalance = second_balance + sell_quantity;
            }

            return buy_quantity;
        }

        public void ResetTokenWithID(DatabaseManager manager)
        {
            if (manager.DynamicProperties.GetAllowSameTokenName() == 0)
            {
                byte[] first_token_name = this.instance.FirstTokenId.ToByteArray();
                byte[] second_token_name = this.instance.SecondTokenId.ToByteArray();
                byte[] first_token_id = first_token_name;
                byte[] second_token_id = second_token_name;
                if (!first_token_name.SequenceEqual(COMPARE_CHARICTOR))
                {
                    first_token_id = Encoding.UTF8.GetBytes(manager.AssetIssue.Get(first_token_name).Id);
                }
                if (!second_token_name.SequenceEqual(COMPARE_CHARICTOR))
                {
                    second_token_id = Encoding.UTF8.GetBytes(manager.AssetIssue.Get(second_token_name).Id);
                }

                this.instance = this.instance ?? new Exchange();
                this.instance.FirstTokenId = ByteString.CopyFrom(first_token_id);
                this.instance.SecondTokenId = ByteString.CopyFrom(second_token_id);
            }
        }
        #endregion
    }
}
