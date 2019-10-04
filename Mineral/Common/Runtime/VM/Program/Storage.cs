using System;
using System.Collections.Generic;
using Mineral.Core.Capsule;
using Mineral.Core.Database;

namespace Mineral.Common.Runtime.VM.Program
{
    public class Storage
    {
        #region Field
        private static readonly int PREFIX_BYTES = 16;

        private byte[] address_hash = null;
        private StorageRowStore storage_row_store = null;
        private readonly Dictionary<DataWord, StorageRowCapsule> row_cache = new Dictionary<DataWord, StorageRowCapsule>();
        #endregion


        #region Property
        #endregion


        #region Contructor
        public Storage(byte[] address, StorageRowStore store)
        {
            this.address_hash = Cryptography.Hash.SHA3(address);
            this.storage_row_store = store;
        }

        public Storage(Storage storage)
        {
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private static byte[] Compose(byte[] key, byte[] address_hash)
        {
            byte[] result = new byte[key.Length];
            Array.Copy(address_hash, 0, result, 0, PREFIX_BYTES);
            Array.Copy(key, PREFIX_BYTES, result, PREFIX_BYTES, PREFIX_BYTES);
            return result;
        }
        #endregion


        #region External Method
        public DataWord Get(DataWord key)
        {
            DataWord result = null;
            if (this.row_cache.ContainsKey(key))
            {
                result = this.row_cache[key].Value;
            }
            else
            {
                StorageRowCapsule row = this.storage_row_store.Get(Compose(key.Data, this.address_hash));
                if (row == null || row.Instance == null)
                {
                    result = null;
                }
                else
                {
                    this.row_cache.Add(key, row);
                    result = row.Value;
                }
            }

            return result;
        }

        public void Put(DataWord key, DataWord value)
        {
            if (this.row_cache.ContainsKey(key))
            {
                this.row_cache[key].Value = value;
            }
            else
            {
                byte[] row_key = Compose(key.Data, this.address_hash);
                this.row_cache.Add(key, new StorageRowCapsule(row_key, value.Data));
            }
        }

        public void Commit()
        {
            foreach (KeyValuePair<DataWord, StorageRowCapsule> pair in this.row_cache)
            {
                if (pair.Value.IsDirty)
                {
                    if (pair.Value.Value.IsZero)
                        this.storage_row_store.Delete(pair.Value.RowKey);
                    else
                        this.storage_row_store.Put(pair.Value.RowKey, pair.Value);
                }
            }
        }
        #endregion
    }
}
