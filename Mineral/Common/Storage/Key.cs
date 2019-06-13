using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Storage
{
    public class Key
    {
        #region Field
        private byte[] data = new byte[0];
        #endregion


        #region Property
        public byte[] Data => this.data;
        #endregion


        #region Contructor
        public Key(Key key)
        {
            this.data = new byte[key.Data.Length];
            Array.Copy(key.Data, 0, this.data, 0, key.Data.Length);
        }

        public Key(byte[] data)
        {
            if (data != null && data.Length > 0)
            {
                this.data = new byte[data.Length];
                Array.Copy(data, 0, this.data, 0, data.Length);
            }
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static Key Create(byte[] data)
        {
            return new Key(data);
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj != null || GetType() != obj.GetType())
                return false;

            Key key = obj as Key;
            return this.data.SequenceEqual(key.Data);
        }

        public Key Clone()
        {
            return new Key(this);
        }
        #endregion
    }
}
