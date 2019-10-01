using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mineral.Core.Database2.Common
{
    public class WrappedByteArray
    {
        #region Field
        private byte[] bytes = null;
        #endregion


        #region Property
        public byte[] Data
        {
            get { return this.bytes; }
        }
        #endregion


        #region Contructor
        public WrappedByteArray(byte[] bytes)
        {
            this.bytes = bytes;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static WrappedByteArray Of(byte[] bytes)
        {
            return new WrappedByteArray(bytes);
        }

        public static WrappedByteArray CopyOf(byte[] bytes)
        {
            byte[] value = new byte[bytes.Length];
            if (bytes != null)
            {
                Array.Copy(bytes, value, bytes.Length);
            }

            return new WrappedByteArray(value);
        }

        public override int GetHashCode()
        {
            return this.bytes.GetHashCode();
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
            WrappedByteArray compare = (WrappedByteArray)obj;

            return this.bytes.SequenceEqual(compare.bytes);
        }
        #endregion

    }
}
