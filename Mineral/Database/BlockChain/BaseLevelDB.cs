using Mineral.Database.LevelDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Database.BlockChain
{
    internal class BaseLevelDB : IDisposable
    {
        protected DB _db = null;

        public BaseLevelDB(string path)
        {
            _db = DB.Open(path, new Options { CreateIfMissing = true });
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        #region TryGet
        public virtual bool TryGetVersion(out Version version)
        {
            Slice value;
            bool result = _db.TryGet(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_Version), out value);
            if (result)
                Version.TryParse(value.ToString(), out version);
            else
                version = null;
            return result;
        }
        #endregion


        #region Get
        public virtual Version GetVersion()
        {
            Slice value;
            Version version;
            value = _db.Get(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_Version));
            Version.TryParse(value.ToString(), out version);
            return version;
        }
        #endregion


        #region Put
        public virtual void PutVersion(Version version)
        {
            _db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_Version), version.ToString());
        }
        #endregion
    }
}
