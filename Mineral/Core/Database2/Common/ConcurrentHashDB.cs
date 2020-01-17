using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Mineral.Utils;

namespace Mineral.Core.Database2.Common
{
    public class ConcurrentHashDB : IBaseDB<byte[], BytesCapsule>
    {
        #region Field
        private ConcurrentDictionary<byte[], BytesCapsule> db = new ConcurrentDictionary<byte[], BytesCapsule>(Environment.ProcessorCount * 2, 50000, new ByteArrayEqualComparer());
        #endregion


        #region Property
        public long Size => this.db.Count;
        public bool IsEmpty => this.db.IsEmpty;
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public BytesCapsule Get(byte[] key)
        {
            this.db.TryGetValue(key, out BytesCapsule result);

            return result;
        }

        public void Put(byte[] key, BytesCapsule value)
        {
            this.db.TryAdd(key, value);
        }

        public void Remove(byte[] key)
        {
            this.db.TryRemove(key, out _);
        }

        public IEnumerator<KeyValuePair<byte[], BytesCapsule>> GetEnumerator()
        {
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return null;
        }
        #endregion
    }
}
