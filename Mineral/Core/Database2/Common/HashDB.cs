using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Stroage.LevelDB;

namespace Mineral.Core.Database2.Common
{
    public class HashDB : IBaseDB<Slice, Slice>
    {
        #region Field
        private ConcurrentDictionary<Slice, Slice> db = new ConcurrentDictionary<Slice, Slice>();
        #endregion


        #region Property
        public long Size { get { return this.db.Count; } }
        public bool IsEmpty { get { return this.db.IsEmpty; } }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public Slice Get(Slice key)
        {
            if (this.db.TryGetValue(key, out Slice value))
                return value;
            else
                return default(Slice);
        }

        public void Put(Slice key, Slice value)
        {
            this.db.TryAdd(key, value);
        }

        public void Remove(Slice key)
        {
            this.db.TryRemove(key, out _);
        }

        public IEnumerator<KeyValuePair<Slice, Slice>> GetEnumerator()
        {
            return this.db.GetEnumerator();
        }
        #endregion
    }
}
