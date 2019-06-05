using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Protocol;

namespace Mineral.Common.Runtime.VM
{
    public class LogInfo
    {
        #region Field
        private byte[] address = new byte[] { };
        private byte[] data = new byte[] { };
        private List<DataWord> topics = new List<DataWord>();
        #endregion


        #region Property
        public byte[] Address { get { return this.address; } set { this.address = value; } }
        public byte[] Data { get { return this.data; } set { this.data = value; } }
        #endregion


        #region Constructor
        public LogInfo(byte[] address, List<DataWord> topics, byte[] data)
        {
            this.address = address ?? new byte[] { };
            this.data = data ?? new byte[] { };
            this.topics = topics ?? new List<DataWord>();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public List<string> ToHexTopics()
        {
            return this.topics.Select(x => x.ToHexString()).ToList();
        }

        public List<byte[]> ToCloneTopics()
        {
            List<byte[]> result = new List<byte[]>();
            this.topics.ForEach(x => result.Add(x.Clone()));

            return result;
        }

        public static TransactionInfo.Types.Log BuildLog(LogInfo info)
        {
            TransactionInfo.Types.Log log = new TransactionInfo.Types.Log();
            log.Address = ByteString.CopyFrom(info.Address);
            log.Data = ByteString.CopyFrom(info.Data);
            log.Topics.AddRange(log.Topics);

            return log;
        }

        public string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("[");

            foreach (DataWord topic in this.topics)
            {
                result.Append(topic.ToHexString()).Append(" ");
            }
            result.Append("]");

            result.Append("LogInfo { ")
                .Append("Address = ").Append(this.address.ToHexString())
                .Append("Data = ").Append(this.data.ToHexString())
                .Append("Topics =").Append(result)
                .Append(" }");

            return result.ToString();
        }
        #endregion
    }
}
