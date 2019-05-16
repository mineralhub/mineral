using Mineral.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mineral.Core2.State
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
            _txResult = reader.ReadByteArray();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteByteArray(_txResult);
        }

        public static TransactionResultState DeserializeFrom(byte[] data, int offset = 0)
        {
            TransactionResultState txResultState = new TransactionResultState();
            using (MemoryStream ms = new MemoryStream(data, offset, data.Length - offset, false))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                txResultState.Deserialize(reader);
            }
            return txResultState;
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["result"] = TxResult.ToString();
            return json;
        }
    }
}
