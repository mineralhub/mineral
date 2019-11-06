using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Cryptography;

namespace Mineral.Core.Database2.Common
{
    public class Key
    {
        #region Field
        private WrappedByteArray data;
        #endregion


        #region Property
        public byte[] Data
        {
            get
            {
                byte[] key = data.Data;
                if (key == null)
                    return null;

                byte[] result = new byte[key.Length];
                Array.Copy(key, result, key.Length);

                return result;
            }
        }
        #endregion


        #region Contructor
        private Key(WrappedByteArray data)
        {
            this.data = data;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static Key CopyOf(byte[] bytes)
        {
            return new Key(WrappedByteArray.CopyOf(bytes));
        }

        public static Key Of(byte[] bytes)
        {
            return new Key(WrappedByteArray.Of(bytes));
        }

        public override int GetHashCode()
        {
            return Hash.SHA256(this.data.Data).ToInt32(0);
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }
            Key compare = (Key)obj;

            return this.data.Equals(compare.Data);
        }
        #endregion
    }
}
