using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class P2pException : System.Exception
    {
        public enum ErrorType
        {
            [Description("no such message")]
            NO_SUCH_MESSAGE = 1,
            [Description("parse message failed")]
            PARSE_MESSAGE_FAILED = 2,
            [Description("message with wrong length")]
            MESSAGE_WITH_WRONG_LENGTH = 3,
            [Description("bad message")]
            BAD_MESSAGE = 4,
            [Description("different genesis block")]
            DIFF_GENESIS_BLOCK = 5,
            [Description("hard forked")]
            HARD_FORKED = 6,
            [Description("sync failed")]
            SYNC_FAILED = 7,
            [Description("check failed")]
            CHECK_FAILED = 8,
            [Description("unlink block")]
            UNLINK_BLOCK = 9,
            [Description("bad block")]
            BAD_BLOCK = 10,
            [Description("bad trx")]
            BAD_TRX = 11,
            [Description("trx exe failed")]
            TRX_EXE_FAILED = 12,
            [Description("DB item not found")]
            DB_ITEM_NOT_FOUND = 13,
            [Description("protobuf inconsistent")]
            PROTOBUF_ERROR = 14,
            [Description("default exception")]
            DEFAULT = 100,
        }

        #region Field
        private ErrorType type = ErrorType.DEFAULT;
        #endregion


        #region Property
        public ErrorType Type { get { return this.type; } }
        #endregion


        #region Constructor
        public P2pException(ErrorType type)
        {
            this.type = type;
        }

        public P2pException(ErrorType type, string message)
            : base(message)
        {
            this.type = type;
        }

        public P2pException(ErrorType type, string message, System.Exception inner)
            : base(message, inner)
        {
            this.type = type;
        }
        protected P2pException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static string GetDescription(ErrorType type)
        {
            FieldInfo info = type.GetType().GetField(type.ToString());
            DescriptionAttribute attr = (DescriptionAttribute)info.GetCustomAttribute(typeof(DescriptionAttribute));

            return attr != null ? attr.Description : "";
        }
        #endregion
    }
}
