using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Net.Messages
{
    public class MessageTypes
    {
        public enum MsgType : byte
        {
            FIRST = 0x00,
            TX = 0x01,
            BLOCK = 0x02,
            TXS = 0x03,
            BLOCKS = 0x04,
            BLOCKHEADERS = 0x05,
            INVENTORY = 0x06,
            FETCH_INV_DATA = 0x07,
            SYNC_BLOCK_CHAIN = 0x08,
            BLOCK_CHAIN_INVENTORY = 0x09,
            ITEM_NOT_FOUND = 0x10,
            FETCH_BLOCK_HEADERS = 0x11,
            BLOCK_INVENTORY = 0x12,
            TX_INVENTORY = 0x13,
            P2P_HELLO = 0x20,
            P2P_DISCONNECT = 0x21,
            P2P_PING = 0x22,
            P2P_PONG = 0x23,
            DISCOVER_PING = 0x30,
            DISCOVER_PONG = 0x31,
            DISCOVER_FIND_PEER = 0x32,
            DISCOVER_PEERS = 0x33,
            LAST = 0xFF,
        }

        private static Dictionary<int, MsgType> messages = new Dictionary<int, MsgType>();

        static MessageTypes()
        {
            foreach (MsgType type in Enum.GetValues(typeof(MsgType)))
                messages.Add((int)type, type);
        }

        public static bool IsP2p(byte code)
        {
            return code <= (byte)MsgType.P2P_PONG && code >= (byte)MsgType.P2P_HELLO;
        }

        public static bool IsMineral(byte code)
        {
            return code <= (byte)MsgType.TX_INVENTORY && code >= (byte)MsgType.FIRST;
        }

        public static MsgType FromByte(byte value)
        {
            MsgType type = MsgType.LAST;

            if (!messages.TryGetValue(value, out type))
                type = MsgType.LAST;

            return type;
        }

    }
}