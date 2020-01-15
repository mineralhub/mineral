using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Storage
{
    public class ValueType
    {
        #region Field
        public static readonly int VALUE_TYPE_NORMAL = 0;
        public static readonly int VALUE_TYPE_DIRTY = 1 << 0;
        public static readonly int VALUE_TYPE_CREATE = 1 << 1;
        public static readonly int VALUE_TYPE_UNKNOWN = unchecked((int)0xFFFFFFFC);

        protected int type = VALUE_TYPE_NORMAL;
        #endregion


        #region Property
        public int Type
        {
            get { return this.type; }
            set { if (IsValidType(value)) this.type = value; }
        }

        public bool IsDirty
        {
            get { return (this.type & VALUE_TYPE_DIRTY) == VALUE_TYPE_DIRTY; }
        }

        public bool IsNormal
        {
            get { return this.type == VALUE_TYPE_NORMAL; }
        }

        public bool IsCreate
        {
            get { return (this.type & VALUE_TYPE_CREATE) == VALUE_TYPE_CREATE; }
        }

        public bool IsShouldCommit
        {
            get { return this.type != VALUE_TYPE_NORMAL; }
        }

        #endregion


        #region Contructor
        public ValueType() { }
        public ValueType(ValueType type)
        {
            this.type = type.Type;
        }

        public ValueType(int type)
        {
            this.type |= type;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public bool IsValidType(int type)
        {
            return (type & VALUE_TYPE_UNKNOWN) == VALUE_TYPE_NORMAL;
        }

        public void AddType(int type)
        {
            this.type |= type;
        }

        public void AddType(ValueType type)
        {
            this.type |= type.Type;
        }

        public ValueType Clone()
        {
            return new ValueType(this);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null || this.GetType() == obj.GetType())
                return false;

            ValueType type = obj as ValueType;
            return this.type == type.Type;
        }

        public override string ToString()
        {
            return "Type {type= = " + this.type + "}";
        }
        #endregion
    }
}
