using Google.Protobuf;
using Mineral.Core.Capsule;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mineral.Core.Database.Api
{
    public class AssetUpdateHelper
    {
        #region Field
        private DatabaseManager db_manager = null;
        private Dictionary<string, byte[]> assets = new Dictionary<string, byte[]>();
        #endregion


        #region Property
        #endregion


        #region Contructor
        public AssetUpdateHelper(DatabaseManager db_manager)
        {
            this.db_manager = db_manager;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Init()
        {
            if (this.db_manager.AssetIssueV2.GetEnumerator().MoveNext())
            {
                Logger.Warning("AssetIssueV2Store is not empty");
            }

            this.db_manager.AssetIssueV2.Reset();

            if (this.db_manager.ExchangeV2.GetEnumerator().MoveNext())
            {
                Logger.Warning("ExchangeV2Store is not empty");
            }

            this.db_manager.ExchangeV2.Reset();
            this.db_manager.DynamicProperties.PutTokenIdNum(1000000);
        }

        public void DoWork()
        {
            Logger.Info("Start updating the asset");
            long start = Helper.CurrentTimeMillis();

            Init();
            UpdateAsset();
            UpdateExchange();
            UpdateAccount();
            Finish();

            Logger.Info(
                string.Format(
                    "Complete the asset update,Total time：{0} milliseconds",
                    Helper.CurrentTimeMillis() - start));
        }

        public List<AssetIssueCapsule> GetAllAssetIssues()
        {
            List<AssetIssueCapsule> result = new List<AssetIssueCapsule>();

            long block_num = 1;
            long latest_header_num = this.db_manager.DynamicProperties.GetLatestBlockHeaderNumber();

            while (block_num <= latest_header_num)
            {
                if (block_num % 100000 == 0)
                {
                    Logger.Info(
                        string.Format("The number of block that have processed：{0}",
                                      block_num));
                }
                try
                {
                    BlockCapsule block = this.db_manager.GetBlockByNum(block_num);
                    foreach (TransactionCapsule tx in block.Transactions)
                    {
                        if (tx.Instance.RawData.Contract[0].Type == Protocol.Transaction.Types.Contract.Types.ContractType.AssetIssueContract)
                        {
                            AssetIssueContract contract = tx.Instance.RawData.Contract[0].Parameter.Unpack<AssetIssueContract>();
                            AssetIssueCapsule asset_issue = new AssetIssueCapsule(contract);

                            result.Add(this.db_manager.AssetIssue.Get(asset_issue.CreateDatabaseKey()));
                        }
                    }
                }
                catch (System.Exception e)
                {
                    throw new System.Exception("Block not exists,num:" + block_num);
                }

                block_num++;
            }

            Logger.Info(
                string.Format("Total block：{0}",
                              block_num));

            if (this.db_manager.AssetIssue.AllAssetIssues.Count != result.Count)
            {
                throw new System.Exception("Asset num is wrong!");
            }

            return result;
        }

        public void UpdateAsset()
        {
            long count = 0;
            long token_num = this.db_manager.DynamicProperties.GetTokenIdNum();

            List<AssetIssueCapsule> assets_issue = GetAllAssetIssues();
            foreach (AssetIssueCapsule asset in assets_issue)
            {
                token_num++;
                count++;

                asset.Id = token_num.ToString();
                this.db_manager.AssetIssue.Put(asset.CreateDatabaseKey(), asset);
                asset.Percision = 0;
                this.db_manager.AssetIssueV2.Put(asset.CreateDatabaseKey(), asset);

                this.assets.Add(Encoding.UTF8.GetString(asset.CreateDatabaseKey()), asset.CreateDatabaseKeyV2());
            }

            this.db_manager.DynamicProperties.PutTokenIdNum(token_num);

            Logger.Info(
                string.Format("Complete the asset store update,Total assets：{0}", count));
        }

        public void UpdateExchange()
        {
            long count = 0;

            foreach (ExchangeCapsule exchange in this.db_manager.Exchange.AllExchanges)
            {
                count++;

                if (!exchange.FirstTokenId.SequenceEqual(Encoding.UTF8.GetBytes("_")))
                {
                    this.assets.TryGetValue(Encoding.UTF8.GetString(exchange.FirstTokenId.ToByteArray()), out byte[] value);
                    exchange.FirstTokenId = ByteString.CopyFrom(value);
                }

                if (!exchange.SecondTokenId.SequenceEqual(Encoding.UTF8.GetBytes("_")))
                {
                    this.assets.TryGetValue(Encoding.UTF8.GetString(exchange.SecondTokenId.ToByteArray()), out byte[] value);
                    exchange.SecondTokenId = ByteString.CopyFrom(value);
                }

                this.db_manager.ExchangeV2.Put(exchange.CreateDatabaseKey(), exchange);
            }

            Logger.Info(
                string.Format(
                    "Complete the exchange store update,Total exchanges：{0}", count));
        }

        public void UpdateAccount()
        {
            long count = 0;
            IEnumerator <KeyValuePair<byte[], AccountCapsule>> it = this.db_manager.Account.GetEnumerator();

            while (it.MoveNext())
            {
                AccountCapsule account = it.Current.Value;

                account.ClearAssetV2();
                if (account.Asset.Count != 0)
                {
                    Dictionary<string, long> dic = new Dictionary<string, long>();
                    foreach (KeyValuePair<string, long> entry in account.Asset)
                    {
                        this.assets.TryGetValue(entry.Key, out byte[] key);
                        dic.Add(Encoding.UTF8.GetString(key), entry.Value);
                    }

                    account.AddAssetV2(dic);
                }

                account.ClearFreeAssetNetUsageV2();
                if (account.FreeAssetNetUsage.Count != 0)
                {
                    Dictionary<string, long> dic = new Dictionary<string, long>();
                    foreach (KeyValuePair<string, long> entry in account.FreeAssetNetUsage)
                    {
                        this.assets.TryGetValue(entry.Key, out byte[] key);
                        dic.Add(Encoding.UTF8.GetString(key), entry.Value);
                    }
                    account.AddAllFreeAssetNetUsageV2(dic);
                }

                account.ClearLatestAssetOperationTimeV2();
                if (account.LatestAssetOperationTime.Count != 0)
                {
                    Dictionary<string, long> dic = new Dictionary<string, long>();
                    foreach (KeyValuePair<string, long> entry in account.LatestAssetOperationTime)
                    {
                        this.assets.TryGetValue(entry.Key, out byte[] key);
                        dic.Add(Encoding.UTF8.GetString(key), entry.Value);
                    }
                    account.AddAllLatestAssetOperationTimeV2(dic);
                }

                if (!account.AssetIssuedName.IsEmpty)
                {
                    this.assets.TryGetValue(Encoding.UTF8.GetString(account.AssetIssuedName.ToByteArray()), out byte[] id);
                    account.AssetIssuedID = ByteString.CopyFrom(id);
                }

                this.db_manager.Account.Put(account.CreateDatabaseKey(), account);

                if (count % 50000 == 0)
                {
                    Logger.Info(
                        string.Format("The number of accounts that have completed the update ： {0}", count));
                }
                count++;
            }

            Logger.Info(
                string.Format("Complete the account store update,Total assets：{0}", count));
        }

        public void Finish()
        {
            this.db_manager.DynamicProperties.PutTokenUpdateDone(1);
            this.assets.Clear();
        }
        #endregion
    }
}
