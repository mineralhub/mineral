using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mineral.Cryptography;

namespace Mineral.Network
{
    public class Message : ISerializable
    {
        public enum CommandName : int
        {
            None = 0,
            Version = 1,
            Verack = 2,
            Ping = 3,
            Pong = 4,

            RequestAddrs = 10,
            ResponseAddrs = 11,

            RequestHeaders = 1000,
            RequestBlocks = 1001,

            ResponseHeaders = 1100,
            ResponseBlocks = 1101,

            BroadcastBlocks = 2000,
            BroadcastTransactions = 2001,

            Alert = 9999,
        }

        private const int PayloadMaxSize = 0x020000000;

        public static uint MagicNumber = Config.Instance.MagicNumber;
        public CommandName Command;
        public uint Checksum;
        public byte[] Payload;

        // magicNumber + CommandLength + payloadLength + checksumSize + payload
        public int Size => sizeof(uint) + sizeof(CommandName) + sizeof(int) + sizeof(uint) + Payload.Length;

        const int HEADER_SIZE = sizeof(uint) + sizeof(CommandName) + sizeof(int) + sizeof(uint);

        public static Message Create(CommandName command, ISerializable payload = null)
        {
            return Create(command, payload == null ? new byte[0] : payload.ToArray());
        }

        public static Message Create(CommandName command, byte[] payload)
        {
            return new Message
            {
                Command = command,
                Checksum = GetChecksum(payload),
                Payload = payload
            };
        }

        public void Deserialize(BinaryReader reader)
        {
            if (MagicNumber != reader.ReadUInt32())
                throw new FormatException();
            Command = (CommandName)reader.ReadInt32();
            int payloadLength = reader.ReadInt32();
            if (payloadLength < 0 || PayloadMaxSize < payloadLength)
                throw new FormatException();
            Checksum = reader.ReadUInt32();
            Payload = reader.ReadBytes(payloadLength);
            if (GetChecksum(Payload) != Checksum)
                throw new FormatException();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(MagicNumber);
            writer.Write((int)Command);
            writer.Write(Payload.Length);
            writer.Write(Checksum);
            writer.Write(Payload);
        }

        private static uint GetChecksum(byte[] value)
        {
            return value.DoubleSHA256().ToUInt32(0);
        }

        public static async Task<Message> DeserializeFromAsync(Stream stream, CancellationToken ctoken)
        {
            int payloadLength;
            byte[] buf = await FillBufferAsync(stream, HEADER_SIZE, ctoken);
            Message message = new Message();
            using (MemoryStream ms = new MemoryStream(buf, false))
            using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8))
            {
                if (reader.ReadUInt32() != MagicNumber)
                    throw new FormatException();
                message.Command = (CommandName)reader.ReadInt32();
                payloadLength = reader.ReadInt32();
                if (payloadLength < 0 || PayloadMaxSize < payloadLength)
                    throw new FormatException();
                message.Checksum = reader.ReadUInt32();
            }
            if (0 < payloadLength)
                message.Payload = await FillBufferAsync(stream, payloadLength, ctoken);
            else
                message.Payload = new byte[0];
            if (GetChecksum(message.Payload) != message.Checksum)
                throw new FormatException();
            return message;
        }

        public static async Task<Message> DeserializeFromAsync(WebSocket ws, CancellationToken ctoken)
        {
            int payloadLength;
            byte[] buf = await FillBufferAsync(ws, HEADER_SIZE, ctoken);
            Message message = new Message();
            using (MemoryStream ms = new MemoryStream(buf, false))
            using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8))
            {
                if (reader.ReadUInt32() != MagicNumber)
                    throw new FormatException();
                message.Command = (CommandName)reader.ReadInt32();
                payloadLength = reader.ReadInt32();
                if (payloadLength > PayloadMaxSize)
                    throw new FormatException();
                message.Checksum = reader.ReadUInt32();
            }
            if (0 < payloadLength)
                message.Payload = await FillBufferAsync(ws, payloadLength, ctoken);
            else
                message.Payload = new byte[0];
            if (GetChecksum(message.Payload) != message.Checksum)
                throw new FormatException();
            return message;
        }

        private static async Task<byte[]> FillBufferAsync(Stream stream, int bufsize, CancellationToken ctoken)
        {
            const int READ_ONCE_SIZE = 1024;
            byte[] buf = new byte[bufsize < READ_ONCE_SIZE ? bufsize : READ_ONCE_SIZE];
            using (MemoryStream ms = new MemoryStream())
            {
                while (0 < bufsize)
                {
                    int read = bufsize < READ_ONCE_SIZE ? bufsize : READ_ONCE_SIZE;
                    read = await stream.ReadAsync(buf, 0, read, ctoken);
                    if (read <= 0)
                        throw new IOException();
                    ms.Write(buf, 0, read);
                    bufsize -= read;
                }
                return ms.ToArray();
            }
        }

        private static async Task<byte[]> FillBufferAsync(WebSocket ws, int bufsize, CancellationToken ctoken)
        {
            const int READ_ONCE_SIZE = 1024;
            byte[] buf = new byte[bufsize < READ_ONCE_SIZE ? bufsize : READ_ONCE_SIZE];
            using (MemoryStream ms = new MemoryStream())
            {
                while (0 < bufsize)
                {
                    int read = bufsize < READ_ONCE_SIZE ? bufsize : READ_ONCE_SIZE;
                    ArraySegment<byte> segment = new ArraySegment<byte>(buf, 0, read);
                    WebSocketReceiveResult result = await ws.ReceiveAsync(segment, ctoken);
                    if (result.Count <= 0 || result.MessageType != WebSocketMessageType.Binary)
                        throw new IOException();
                    ms.Write(buf, 0, read);
                    bufsize -= read;
                }
                return ms.ToArray();
            }
        }
    }
}
