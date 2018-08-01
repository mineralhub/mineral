using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Numerics;
using System.Globalization;

namespace Sky
{
    public static class Helper
    {
        private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe internal static int ToInt32(this byte[] value, int startIndex)
        {
            fixed (byte* pbyte = &value[startIndex])
            {
                return *((int*)pbyte);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe internal static uint ToUInt32(this byte[] value, int startIndex)
        {
            fixed (byte* pbyte = &value[startIndex])
            {
                return *((uint*)pbyte);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe internal static ushort ToUInt16(this byte[] value, int startIndex)
        {
            fixed (byte* pbyte = &value[startIndex])
            {
                return *((ushort*)pbyte);
            }
        }

        private static int BitLen(int w)
        {
            return (w < 1 << 15 ? (w < 1 << 7
                ? (w < 1 << 3 ? (w < 1 << 1
                ? (w < 1 << 0 ? (w < 0 ? 32 : 0) : 1)
                : (w < 1 << 2 ? 2 : 3)) : (w < 1 << 5
                ? (w < 1 << 4 ? 4 : 5)
                : (w < 1 << 6 ? 6 : 7)))
                : (w < 1 << 11
                ? (w < 1 << 9 ? (w < 1 << 8 ? 8 : 9) : (w < 1 << 10 ? 10 : 11))
                : (w < 1 << 13 ? (w < 1 << 12 ? 12 : 13) : (w < 1 << 14 ? 14 : 15)))) : (w < 1 << 23 ? (w < 1 << 19
                ? (w < 1 << 17 ? (w < 1 << 16 ? 16 : 17) : (w < 1 << 18 ? 18 : 19))
                : (w < 1 << 21 ? (w < 1 << 20 ? 20 : 21) : (w < 1 << 22 ? 22 : 23))) : (w < 1 << 27
                ? (w < 1 << 25 ? (w < 1 << 24 ? 24 : 25) : (w < 1 << 26 ? 26 : 27))
                : (w < 1 << 29 ? (w < 1 << 28 ? 28 : 29) : (w < 1 << 30 ? 30 : 31)))));
        }

        internal static int GetLowestSetBit(this BigInteger i)
        {
            if (i.Sign == 0)
                return -1;
            byte[] b = i.ToByteArray();
            int w = 0;
            while (b[w] == 0)
                w++;
            for (int x = 0; x < 8; x++)
                if ((b[w] & 1 << x) > 0)
                    return x + w * 8;
            throw new Exception();
        }

        public static string ToHexString(this IEnumerable<byte> value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in value)
                sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }

        public static int ToTimestamp(this DateTime time)
        {
            return (int)(time.ToUniversalTime() - unixEpoch).TotalSeconds;
        }

        public static byte[] HexToBytes(this string value)
        {
            if (value == null || value.Length == 0)
                throw new FormatException();
            if (value.Length % 2 == 1)
                throw new FormatException();
            byte[] result = new byte[value.Length / 2];
            for (int i = 0; i < result.Length; i++)
                result[i] = byte.Parse(value.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
            return result;
        }

        public static void WriteFixedString(this BinaryWriter writer, string value, int length)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (value.Length > length)
                throw new ArgumentException();
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length > length)
                throw new ArgumentException();
            writer.Write(bytes);
            if (bytes.Length < length)
                writer.Write(new byte[length - bytes.Length]);
        }

        public static string ReadFixedString(this BinaryReader reader, int length)
        {
            byte[] data = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(data.TakeWhile(p => p != 0).ToArray());
        }

        internal static int GetBitLength(this BigInteger i)
        {
            byte[] b = i.ToByteArray();
            return (b.Length - 1) * 8 + BitLen(i.Sign > 0 ? b[b.Length - 1] : 255 - b[b.Length - 1]);
        }

        internal static BigInteger NextBigInteger(this RandomNumberGenerator rng, int sizeInBits)
        {
            if (sizeInBits < 0)
                throw new ArgumentException("sizeInBits must be non-negative");
            if (sizeInBits == 0)
                return 0;
            byte[] b = new byte[sizeInBits / 8 + 1];
            rng.GetBytes(b);
            if (sizeInBits % 8 == 0)
                b[b.Length - 1] = 0;
            else
                b[b.Length - 1] &= (byte)((1 << sizeInBits % 8) - 1);
            return new BigInteger(b);
        }

        internal static BigInteger Mod(this BigInteger x, BigInteger y)
        {
            x %= y;
            if (x.Sign < 0)
                x += y;
            return x;
        }

        internal static BigInteger ModInverse(this BigInteger a, BigInteger n)
        {
            BigInteger i = n, v = 0, d = 1;
            while (a > 0)
            {
                BigInteger t = i / a, x = a;
                a = i % x;
                i = x;
                x = d;
                d = v - t * x;
                v = x;
            }
            v %= n;
            if (v < 0) v = (v + n) % n;
            return v;
        }

        internal static bool TestBit(this BigInteger i, int index)
        {
            return (i & (BigInteger.One << index)) > BigInteger.Zero;
        }

        internal static BigInteger NextBigInteger(this Random rand, int sizeInBits)
        {
            if (sizeInBits < 0)
                throw new ArgumentException("sizeInBits must be non-negative");
            if (sizeInBits == 0)
                return 0;
            byte[] b = new byte[sizeInBits / 8 + 1];
            rand.NextBytes(b);
            if (sizeInBits % 8 == 0)
                b[b.Length - 1] = 0;
            else
                b[b.Length - 1] &= (byte)((1 << sizeInBits % 8) - 1);
            return new BigInteger(b);
        }

        private static int GetSize<T>(Type type, IEnumerable<T> value)
        {
            if (typeof(ISerializable).IsAssignableFrom(type))
            {
                return value.OfType<ISerializable>().Sum(p => p.Size);
            }
            else if (type.IsEnum)
            {
                return Marshal.SizeOf(Enum.GetUnderlyingType(type)) * value.Count();
            }
            else
            {
                return value.Count() * Marshal.SizeOf<T>();
            }
        }

        public static int GetSize<T>(this List<T> value)
        {
            return sizeof(int) + GetSize(typeof(T), value);
        }

        public static int GetSize<T>(this T[] value)
        {
            return sizeof(int) + GetSize(typeof(T), value);
        }

        public static int GetSize<TKey, TValue>(this Dictionary<TKey, TValue> value)
        {
            return sizeof(int) + GetSize(typeof(TKey), value.Keys.ToList()) + GetSize(typeof(TValue), value.Values.ToList());
        }

        public static void WriteSerializable(this BinaryWriter writer, ISerializable value)
        {
            value.Serialize(writer);
        }

        public static void WriteSerializableArray<T>(this BinaryWriter writer, IEnumerable<T> value) where T : ISerializable
        {
            writer.Write(value.Count());
            foreach (T v in value)
                writer.WriteSerializable(v);
        }

        public static void WriteSerializableDictonary<TKey, TValue>(this BinaryWriter writer, IEnumerable<KeyValuePair<TKey, TValue>> value) where TKey : ISerializable where TValue : ISerializable
        {
            writer.Write(value.Count());
            var e = value.GetEnumerator();
            while (e.MoveNext())
            {
                writer.WriteSerializable(e.Current.Key);
                writer.WriteSerializable(e.Current.Value);
            }
        }

        public static void WriteByteArray(this BinaryWriter writer, IEnumerable<byte> value)
        {
            if (value == null || value.Count() == 0)
            {
                writer.Write(0);
                return;
            }

            writer.Write(value.Count());
            foreach (byte v in value)
                writer.Write(v);
        }

        public static List<T> ReadSerializableArray<T>(this BinaryReader reader, int maxCount = int.MaxValue) where T : ISerializable, new()
        {
            int count = reader.ReadInt32();
            if (maxCount < count)
                count = maxCount;

            List<T> list = new List<T>(count);
            for (int i = 0; i < list.Capacity; ++i)
            {
                list.Add(new T());
                list[i].Deserialize(reader);
            }
            return list;
        }

        public static Dictionary<TKey, TValue> ReadSerializableDictionary<TKey, TValue>(this BinaryReader reader, int maxCount = int.MaxValue) where TKey : ISerializable, new() where TValue : ISerializable, new()
        {
            int count = reader.ReadInt32();
            if (maxCount < count)
                count = maxCount;

            Dictionary<TKey, TValue> list = new Dictionary<TKey, TValue>(count);
            for (int i = 0; i < count; ++i)
            {
                TKey k = new TKey();
                TValue v = new TValue();
                k.Deserialize(reader);
                v.Deserialize(reader);
                list.Add(k, v);
            }
            return list;
        }

        public static T ReadSerializable<T>(this BinaryReader reader) where T : ISerializable, new()
        {
            T obj = new T();
            obj.Deserialize(reader);
            return obj;
        }

        public static byte[] ReadByteArray(this BinaryReader reader)
        {
            int count = reader.ReadInt32();
            if (count == 0)
                return null;

            byte[] array = new byte[count];
            for (int i = 0; i < array.Length; ++i)
            {
                array[i] = reader.ReadByte();
            }
            return array;
        }

        public static byte[] ToArray(this ISerializable value)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8))
            {
                value.Serialize(writer);
                writer.Flush();
                return ms.ToArray();
            }
        }

        public static byte[] ToByteArray<T>(this T[] value) where T : ISerializable
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8))
            {
                writer.WriteSerializableArray(value);
                writer.Flush();
                return ms.ToArray();
            }
        }

        public static T Serializable<T>(this byte[] value) where T: ISerializable, new()
        {
            using (MemoryStream ms = new MemoryStream(value, false))
            using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8))
            {
                return reader.ReadSerializable<T>();
            }
        }

        public static List<T> SerializableArray<T>(this byte[] value, int maxCount = int.MaxValue) where T: ISerializable, new()
        {
            using (MemoryStream ms = new MemoryStream(value, false))
            using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8))
            {
                return reader.ReadSerializableArray<T>(maxCount);
            }
        }

        public static Fixed8 Sum<T>(this IEnumerable<T> source, Func<T, Fixed8> selector)
        {
            return source.Select(selector).Sum();
        }

        public static Fixed8 Sum(this IEnumerable<Fixed8> source)
        {
            long sum = 0;
            checked
            {
                foreach (Fixed8 item in source)
                {
                    sum += item.Value;
                }
            }
            return new Fixed8(sum);
        }
    }
}
