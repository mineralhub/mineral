using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Mineral;
using Mineral.Common.Runtime.VM;
using Mineral.Core;
using Mineral.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protocol;
using static MineralCLI.Util.AbiUtil.Coder;
using static Protocol.SmartContract.Types;

namespace MineralCLI.Util
{
    public class AbiUtil
    {
        private static Regex PatternBytes = new Regex("^bytes([0-9]*)$");
        private static Regex PatternNumber = new Regex("^(u?int)([0-9]*)$");
        private static Regex PatternArray = new Regex("^(.*)\\[([0-9]*)]$");

        public abstract class Coder
        {
            public bool Dynamic = false;

            public abstract byte[] Encode(string value);
            public abstract byte[] Decode();

            protected static byte[] EncodeDynamicBytes(string value)
            {
                byte[] data = Encoding.UTF8.GetBytes(value);
                List<DataWord> result = new List<DataWord>();
                result.Add(new DataWord(data.Length));

                return EncodeDynamicBytes(data);
            }

            protected static byte[] EncodeDynamicBytes(byte[] data)
            {
                List<DataWord> result = new List<DataWord>();
                result.Add(new DataWord(data.Length));

                int index = 0;
                int len = data.Length;

                while (index < data.Length)
                {
                    byte[] word_data = new byte[32];
                    int len_read = len - index >= 32 ? 32 : (len - index);

                    Array.Copy(data, index, word_data, 0, len_read);
                    DataWord word = new DataWord(word_data);

                    result.Add(word);
                    index += 32;
                }

                int index_result = 0;
                byte[] bytes = new byte[result.Count * 32];

                foreach (DataWord w in result)
                {
                    Array.Copy(w.Data, 0, bytes, index_result, 32);
                    index_result += 32;
                }

                return bytes;
            }

            private static byte[] EncodeDynamicBytes(string value, bool is_hex)
            {
                byte[] data;
                if (is_hex)
                {
                    if (value.StartsWith("0x"))
                    {
                        value = value.Substring(2);
                    }

                    data = Mineral.Helper.HexToBytes(value);
                }
                else
                {
                    data = Encoding.UTF8.GetBytes(value);
                }

                return EncodeDynamicBytes(data);
            }

            public class CoderAddress : Coder
            {
                public override byte[] Encode(string value)
                {
                    byte[] address = Wallet.Base58ToAddress(value);
                    if (address == null)
                    {
                        return null;
                    }
                    return new DataWord(address).Data;
                }

                public override byte[] Decode()
                {
                    return new byte[0];
                }
            }

            public class CoderString : Coder
            {
                public CoderString()
                {
                    this.Dynamic = true;
                }

                public override byte[] Encode(String value)
                {
                    return EncodeDynamicBytes(value);
                }

                public override byte[] Decode()
                {
                    return new byte[0];
                }
            }

            public class CoderBool : Coder
            {
                public override byte[] Encode(string value)
                {
                    byte[] result = new DataWord(0).Data;

                    if (value.Equals("true") || !value.Equals("0"))
                    {
                        result = new DataWord(1).Data;
                    }

                    return result;
                }

                public override byte[] Decode()
                {
                    return new byte[0];
                }
            }

            public class CoderDynamicBytes : Coder
            {
                public CoderDynamicBytes()
                {
                    this.Dynamic = true;
                }

                public override byte[] Encode(string value)
                {
                    return EncodeDynamicBytes(value, true);
                }

                public override byte[] Decode()
                {
                    return new byte[0];
                }
            }

            public class CoderNumber : Coder
            {
                public override byte[] Encode(string value)
                {
                    long n = long.Parse(value);
                    DataWord word = new DataWord(Math.Abs(n));
                    if (n < 0)
                    {
                        word.Negate();
                    }
                    return word.Data;
                }

                public override byte[] Decode()
                {
                    return new byte[0];
                }
            }

            public class CoderFixedBytes : Coder
            {
                public override byte[] Encode(string value)
                {
                    if (value.StartsWith("0x"))
                    {
                        value = value.Substring(2);
                    }

                    if (value.Length % 2 != 0)
                    {
                        value = "0" + value;
                    }

                    byte[] result = new byte[32];
                    byte[] bytes = Mineral.Helper.HexToBytes(value);

                    Array.Copy(bytes, 0, result, 0, bytes.Length);

                    return result;
                }

                public override byte[] Decode()
                {
                    return new byte[0];
                }
            }

            public class CoderArray : Coder
            {
                private int length;
                private string element_type;

                public CoderArray(string array_type, int length)
                {
                    this.length = length;
                    this.element_type = array_type;
                    this.Dynamic = true;
                }

                public override byte[] Encode(string array_values)
                {
                    Coder coder = GetParameterCoder(this.element_type);
                    List<string> values = null;

                    try
                    {
                        values = ParameterToList(array_values);
                    }
                    catch (System.Exception e)
                    {
                        throw e;
                    }

                    List<Coder> coders = new List<Coder>();

                    if (this.length == -1)
                    {
                        for (int i = 0; i < values.Count; i++)
                        {
                            coders.Add(coder);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < this.length; i++)
                        {
                            coders.Add(coder);
                        }
                    }

                    if (this.length == -1)
                    {
                        byte[] items1 = new DataWord(values.Count).Data;
                        byte[] items2 = Pack(coders, (object[])values.ToArray());

                        byte[] merge = new byte[items1.Length + items2.Length];
                        Array.Copy(items1, 0, merge, 0, items1.Length);
                        Array.Copy(items2, 0, merge, items1.Length + 1, items2.Length);

                        return merge;
                    }
                    else
                    {
                        return Pack(coders, (object[])values.ToArray());
                    }
                }

                public override byte[] Decode()
                {
                    return new byte[0];
                }
            }
        }

        #region Field
        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private static List<string> ParameterToList(string input)
        {
            string[] parameters = input.Split(',');

            List<string> result = new List<string>();
            foreach (string item in parameters)
            {
                result.Add(item.Trim());
            }

            return result;
        }

        private static Coder GetParameterCoder(string type)
        {
            switch (type)
            {
                case "address":
                    return new CoderAddress();
                case "string":
                    return new CoderString();
                case "bool":
                    return new CoderBool();
                case "bytes":
                    return new CoderDynamicBytes();
                case "trcToken":
                    return new CoderNumber();
            }

            if (PatternBytes.Match(type).Success)
            {
                return new CoderFixedBytes();
            }

            if (PatternNumber.Match(type).Success)
            {
                return new CoderNumber();
            }

            Match match = PatternArray.Match(type);
            if (match.Success)
            {
                int length = -1;
                string array_type = match.Groups[1].Value;

                if (!match.Groups[2].Equals(""))
                {
                    length = int.Parse(match.Groups[2].Value);
                }

                return new CoderArray(array_type, length);
            }

            return null;
        }
        #endregion


        #region External Method
        #endregion

        public static string[] GetTypes(string construct)
        {
            int start = construct.IndexOf('(') + 1;
            int end = construct.IndexOf(')');
            string parameters = construct.Substring(start, end);

            return parameters.Split(",");
        }

        public static byte[] EncodeInput(string construct, string parameters)
        {
            List<string> items = ParameterToList(parameters);

            List<Coder> coders = new List<Coder>();
            foreach (string p in GetTypes(construct))
            {
                Coder coder = GetParameterCoder(p);
                coders.Add(coder);
            }

            return Pack(coders, (object[])items.ToArray());
        }

        public static byte[] Pack(List<Coder> codes, object[] values)
        {
            int size_static = 0;
            int size_dynamic = 0;

            List<byte[]> encodeds = new List<byte[]>();

            for (int i = 0; i < codes.Count; i++)
            {
                string value = "";
                Coder coder = codes[i];
                object parameter = values[i];

                if (parameter.GetType().IsGenericType && (parameter.GetType().GetGenericTypeDefinition() == typeof(List<>)))
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (object item in (List<object>)parameter)
                    {
                        if (sb.Length != 0)
                        {
                            sb.Append(",");
                        }
                        sb.Append("\"").Append(item).Append("\"");
                    }
                    value = "[" + sb.ToString() + "]";
                }
                else
                {
                    value = parameter.ToString();
                }

                byte[] encoded = coder.Encode(value);
                encodeds.Add(encoded);

                if (coder.Dynamic)
                {
                    size_static += 32;
                    size_dynamic += encoded.Length;
                }
                else
                {
                    size_static += encoded.Length;
                }
            }

            int offset = 0;
            int offset_dynamic = size_static;
            byte[] data = new byte[size_static + size_dynamic];

            for (int i = 0; i < codes.Count; i++)
            {
                Coder coder = codes[i];
                if (coder.Dynamic)
                {
                    Array.Copy(new DataWord(offset_dynamic).Data, 0, data, offset, 32);
                    offset += 32;

                    Array.Copy(encodeds[i], 0, data, offset_dynamic, encodeds[i].Length);
                    offset_dynamic += encodeds[i].Length;
                }
                else
                {
                    Array.Copy(encodeds[i], 0, data, offset, encodeds[i].Length);
                    offset += encodeds[i].Length;
                }
            }

            return data;
        }

        public static ABI.Types.Entry.Types.StateMutabilityType GetStateMutability(string state)
        {
            switch (state)
            {
                case "pure":
                    return ABI.Types.Entry.Types.StateMutabilityType.Pure;
                case "view":
                    return ABI.Types.Entry.Types.StateMutabilityType.View;
                case "nonpayable":
                    return ABI.Types.Entry.Types.StateMutabilityType.Nonpayable;
                case "payable":
                    return ABI.Types.Entry.Types.StateMutabilityType.Payable;
                default:
                    return ABI.Types.Entry.Types.StateMutabilityType.UnknownMutabilityType;
            }
        }

        public static string ParseMethod(string method, string parameters)
        {
            return ParseMethod(method, parameters, false);
        }

        public static string ParseMethod(string method, string parameters, bool is_hex)
        {
            byte[] selector = new byte[4];
            Array.Copy(Hash.SHA3(Encoding.UTF8.GetBytes(method)), 0, selector, 0, 4);
            Console.WriteLine(method + ":" + selector.ToHexString());
            if (parameters.Length == 0)
            {
                return selector.ToHexString();
            }

            if (is_hex)
            {
                return selector.ToHexString() + parameters;
            }

            byte[] encoded = EncodeInput(method, parameters);

            return selector.ToHexString() + encoded.ToHexString();
        }

        public static ABI JsonToAbi(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException("json string is null.");
            }

            ABI abi = new ABI();

            try
            {
                JArray array = JArray.Parse(json);
                foreach (JToken item in array)
                {
                    AbiEntity entity = JsonConvert.DeserializeObject<AbiEntity>(item.ToString());

                    if (entity.AbiType == null)
                    {
                        throw new ArgumentNullException("Abi type is null.");
                    }

                    if (!entity.AbiType.ToLower().Equals("fallback") && entity.Inputs == null)
                    {
                        throw new ArgumentException("Abi inputs is empty.");
                    }

                    ABI.Types.Entry abi_entity = new ABI.Types.Entry();
                    abi_entity.Anonymous = entity.Anonymous;
                    abi_entity.Constant = entity.Constant;
                    if (entity.Name != null)
                    {
                        abi_entity.Name = entity.Name;
                    }

                    foreach (var input in entity.Inputs)
                    {
                        abi_entity.Inputs.Add(new ABI.Types.Entry.Types.Param()
                        {
                            Name = input.Name,
                            Type = input.InOutType,
                            Indexed = input.Indexed,
                        });
                    }

                    foreach (var input in entity.Outputs)
                    {
                        abi_entity.Outputs.Add(new ABI.Types.Entry.Types.Param()
                        {
                            Name = input.Name,
                            Type = input.InOutType,
                            Indexed = input.Indexed,
                        });
                    }

                    abi.Entrys.Add(abi_entity);
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }

            return abi;
        }
    }
}
