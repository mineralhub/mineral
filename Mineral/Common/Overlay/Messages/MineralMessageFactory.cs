using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Exception;
using Mineral.Core.Net.Messages;
using Mineral.Utils;

namespace Mineral.Common.Overlay.Messages
{
    public class MineralMessageFactory : MessageFactory
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private Message Create(byte type, byte[] packed)
        {
            MessageTypes.MsgType received = MessageTypes.FromByte(type);
            if (received == MessageTypes.MsgType.LAST)
            {
                throw new P2pException(P2pException.ErrorType.NO_SUCH_MESSAGE, "type=" + type + ", len=" + packed.Length);
            }

            switch (received)
            {
                case MessageTypes.MsgType.TX:
                    return new TransactionMessage(packed);
                case MessageTypes.MsgType.BLOCK:
                    return new BlockMessage(packed);
                case MessageTypes.MsgType.TXS:
                    return new TransactionsMessage(packed);
                case MessageTypes.MsgType.BLOCKS:
                    return new BlocksMessage(packed);
                case MessageTypes.MsgType.INVENTORY:
                    return new InventoryMessage(packed);
                case MessageTypes.MsgType.FETCH_INV_DATA:
                    return new FetchInventoryDataMessage(packed);
                case MessageTypes.MsgType.SYNC_BLOCK_CHAIN:
                    return new SyncBlockChainMessage(packed);
                case MessageTypes.MsgType.BLOCK_CHAIN_INVENTORY:
                    return new ChainInventoryMessage(packed);
                case MessageTypes.MsgType.ITEM_NOT_FOUND:
                    return new ItemNotFoundMessage();
                case MessageTypes.MsgType.FETCH_BLOCK_HEADERS:
                    return new FetchBlockHeadersMessage(packed);
                case MessageTypes.MsgType.TX_INVENTORY:
                    return new TransactionInventoryMessage(packed);
                default:
                    throw new P2pException(
                        P2pException.ErrorType.NO_SUCH_MESSAGE, received.ToString() + ", len=" + packed.Length);
            }
        }
        #endregion


        #region External Method
        public override Message Create(byte[] data)
        {
            try
            {
                byte type = data[0];
                byte[] raw_data = ArrayUtil.SubArray(data, 1, data.Length);

                return Create(type, raw_data);
            }
            catch (P2pException e)
            {
                throw e;
            }
            catch (System.Exception e)
            {
                throw new P2pException(
                    P2pException.ErrorType.PARSE_MESSAGE_FAILED,
                    "type=" + data[0] + ", len=" + data.Length + ", error msg: " + e.Message);
            }
        }
        #endregion
    }
}
