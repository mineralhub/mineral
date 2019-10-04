using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Runtime.VM;
using Mineral.Common.Utils;

namespace Mineral.Core.Capsule
{
    public class StorageRowCapsule : IProtoCapsule<byte[]>
    {
        #region Field
        private byte[] row_key;
        private byte[] row_value;
        private bool dirty = false;
        #endregion


        #region Property
        public byte[] Instance => this.row_value;
        public byte[] Data => this.row_value;

        public byte[] RowKey { get => row_key; set => row_key = value; }
        public byte[] RowValue => this.row_value;
        public bool IsDirty => this.dirty;

        public SHA256Hash Hash
        {
            get { return SHA256Hash.Of(this.row_value); }
        }

        public DataWord Value
        {
            get { return new DataWord(this.row_value); }
            set { this.row_value = value.Data; this.dirty = true; }
        }
        #endregion


        #region Contructor
        public StorageRowCapsule(StorageRowCapsule storage_row)
        {
            this.row_key = storage_row.row_key;
            this.row_value = storage_row.row_value;
            this.dirty = storage_row.dirty;
        }

        public StorageRowCapsule(byte[] row_key, byte[] row_value)
        {
            this.row_key = row_key;
            this.row_value = row_value;
            this.dirty = true;
        }

        public StorageRowCapsule(byte[] row_value)
        {
            this.row_value = row_value;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override string ToString()
        {
            return this.row_value.ToString();
        }
        #endregion
    }
}
