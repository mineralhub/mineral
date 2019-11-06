using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Cryptography;

namespace Mineral.Core.Database2.Common
{
    public class Value
    {
        public enum Operator : byte
        {
            CREATE = 0,
            MODIFY,
            DELETE,
            PUT
        }

        #region Field
        private Operator op;
        private WrappedByteArray data = null;
        #endregion


        #region Property
        public byte[] Data
        {
            get
            {
                byte[] value = data.Data;
                if (value == null)
                {
                    return null;
                }

                byte[] result = new byte[value.Length];
                Array.Copy(value, result, value.Length);

                return result;
            }
        }
        #endregion


        #region Contructor
        private Value(Operator op, WrappedByteArray data)
        {
            this.op = op;
            this.data = data;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public byte[] Encode()
        {
            if (data.Data == null)
            {
                return new byte[] { (byte)op };
            }

            byte[] result = new byte[1 + data.Data.Length];
            result[0] = (byte)op;
            Array.Copy(data.Data, 0, result, 1, data.Data.Length);

            return result;
        }

        public static Value Decode(byte[] bytes)
        {
            Operator op = (Operator)bytes[0];
            byte[] value = null;
            if (bytes.Length > 1)
            {
                Array.Copy(bytes, 1, value, 0, bytes.Length);
            }

            return Value.Of(op, value);
        }

        public static Value CopyOf(Operator op, byte[] data)
        {
            return new Value(op, WrappedByteArray.CopyOf(data));
        }

        public static Value Of(Operator op, byte[] data)
        {
            return new Value(op, WrappedByteArray.Of(data));
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
            Value compare = (Value)obj;

            return this.data.Equals(compare.Data);
        }
        #endregion
    }
}
