using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Database.Fast;

namespace Mineral.Core.Tire
{
    public class ScanAction : IScanAction
    {
        #region Field
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
        public void OnNode(byte[] hash, TrieNode node)
        {
        }

        public void OnValue(byte[] node_hash, TrieNode node, byte[] key, byte[] value)
        {
            try
            {
                Logger.Info(string.Format("Account info : {0}", AccountStateEntity.Parse(value)));
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }
        }
        #endregion
    }
}
