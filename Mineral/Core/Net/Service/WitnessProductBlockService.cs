using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;
using System.Linq;
using Mineral.Core.Capsule;

namespace Mineral.Core.Net.Service
{
    public class WitnessProductBlockService
    {
        #region Field
        private MemoryCache history_block_cache = MemoryCache.Default;
        private Dictionary<string, CheatWitnessInfo> cheat_witnesses = new Dictionary<string, CheatWitnessInfo>();
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void ValidWitnessProductTwoBlock(BlockCapsule block)
        {
            try
            {
                BlockCapsule history_block = (BlockCapsule)this.history_block_cache.Get(block.Num.ToString());

                if (history_block != null
                    && history_block.WitnessAddress.ToByteArray().SequenceEqual(block.WitnessAddress.ToByteArray())
                    && !block.Id.Hash.SequenceEqual(history_block.Id.Hash))
                {
                    string key = block.WitnessAddress.ToByteArray().ToHexString();
                    if (!this.cheat_witnesses.TryGetValue(key, out CheatWitnessInfo value))
                    {
                        CheatWitnessInfo cheat_witness = new CheatWitnessInfo();
                        this.cheat_witnesses.Add(key, cheat_witness);
                    }

                    value.Clear();
                    value.Time = Helper.CurrentTimeMillis();
                    value.LatestBlockNum = block.Num;
                    value.Add(block);
                    value.Add(history_block);
                    value.Increment();
                }
                else
                {
                    this.history_block_cache.Add(block.Num.ToString(), block, new CacheItemPolicy());
                }
            }
            catch (System.Exception)
            {
                Logger.Error(
                    string.Format("valid witness same time product two block fail! blockNum: {0}, blockHash: {1}",
                                  block.Num,
                                  block.Id.ToString()));
            }
        }
        #endregion
    }
}
