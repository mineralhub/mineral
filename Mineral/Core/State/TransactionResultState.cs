using Mineral.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mineral.Core.State
{
    public class TransactionResultState : StateBase
    {
        private byte[] _txResult;

        public MINERAL_ERROR_CODES TxResult
        {
            get { return (MINERAL_ERROR_CODES)BitConverter.ToInt64(_txResult, 0); }
            private set { _txResult = BitConverter.GetBytes((Int64)value).Take(8).ToArray(); }
        }

        public override int Size => base.Size + sizeof(MINERAL_ERROR_CODES);

        public TransactionResultState()
        {
        }

        public TransactionResultState(MINERAL_ERROR_CODES txResult)
        {
            TxResult = txResult;
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            TxResult = (MINERAL_ERROR_CODES)reader.ReadInt64();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteByteArray(_txResult);
        }
    }
}
