using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mineral.Core.Capsule;
using Mineral.Utils;

namespace Mineral.Common.Storage
{
    public class Value
    {
        #region Field
        private byte[] data = new byte[0];
        private ValueType type = null;
        #endregion


        #region Property
        public byte[] Data => this.data;
        public ValueType Type
        {
            get { return this.type; }
            set { this.type = value; }
        }
        #endregion


        #region Contructor
        public Value(Value value)
        {
            if (value.Data != null && value.Data.Length > 0)
            {
                this.data = new byte[value.Data.Length];
                Array.Copy(value.Data, 0, this.data, 0, value.data.Length);
                this.type = new ValueType(value.Type);
            }
            else
            {
                if (Common.Runtime.Config.VMConfig.AllowMultiSign)
                {
                    this.type = new ValueType(ValueType.VALUE_TYPE_UNKNOWN);
                }
            }
        }

        public Value(byte[] data, ValueType type)
        {
            if (data != null && data.Length > 0)
            {
                this.data = new byte[data.Length];
                Array.Copy(data, 0, this.data, 0, data.Length);
                this.type = type.Clone();
            }
        }

        public Value(byte[] data, int type)
        {
            if (data != null && data.Length > 0)
            {
                this.data = new byte[data.Length];
                Array.Copy(data, 0, this.data, 0, data.Length);
                this.type = new ValueType(type);
            }
            else
            {
                if (Common.Runtime.Config.VMConfig.AllowMultiSign)
                {
                    this.type = new ValueType(ValueType.VALUE_TYPE_UNKNOWN);
                }
            }
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static Value Create(byte[] data)
        {
            return new Value(data, ValueType.VALUE_TYPE_NORMAL);
        }

        public static Value Create(byte[] data, int type)
        {
            return new Value(data, type);
        }

        public void AddType(ValueType type)
        {
            this.type.AddType(type);
        }

        public void AddType(int type)
        {
            this.type.AddType(type);
        }

        public T ToCapsule<T, U>() where T : class, IProtoCapsule<U>
        {
            T result = null;
            try
            {
                if (this.data.IsNotNullOrEmpty())
                {
                    result = (T)Activator.CreateInstance(typeof(T), new object[] { this.data });
                }
            }
            catch
            {
                result = null;
            }

            return result;
        }

        public Value Clone()
        {
            return new Value(this);
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null || this.GetType() != obj.GetType())
                return false;

            Value value = obj as Value;
            return this.data.SequenceEqual(value.Data);
        }
        #endregion
    }
}
