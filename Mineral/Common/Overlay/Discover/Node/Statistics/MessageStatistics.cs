using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Net.Udp.Message;
using Mineral.Common.Overlay.Messages;
using Mineral.Core.Net.Messages;

namespace Mineral.Common.Overlay.Discover.Node.Statistics
{
    using Message = Common.Overlay.Messages.Message;

    public class MessageStatistics
    {
        #region Field
        //udp discovery
        public readonly MessageCount DiscoverInPing = new MessageCount();
        public readonly MessageCount DiscoverOutPing = new MessageCount();
        public readonly MessageCount DiscoverInPong = new MessageCount();
        public readonly MessageCount DiscoverOutPong = new MessageCount();
        public readonly MessageCount DiscoverInFindNode = new MessageCount();
        public readonly MessageCount DiscoverOutFindNode = new MessageCount();
        public readonly MessageCount DiscoverInNeighbours = new MessageCount();
        public readonly MessageCount DiscoverOutNeighbours = new MessageCount();

        //tcp p2p
        public readonly MessageCount P2pInHello = new MessageCount();
        public readonly MessageCount P2pOutHello = new MessageCount();
        public readonly MessageCount P2pInPing = new MessageCount();
        public readonly MessageCount P2pOutPing = new MessageCount();
        public readonly MessageCount P2pInPong = new MessageCount();
        public readonly MessageCount P2pOutPong = new MessageCount();
        public readonly MessageCount P2pInDisconnect = new MessageCount();
        public readonly MessageCount P2pOutDisconnect = new MessageCount();

        //tcp mineral
        public readonly MessageCount MineralInMessage = new MessageCount();
        public readonly MessageCount MineralOutMessage = new MessageCount();

        public readonly MessageCount MineralInSyncBlockChain = new MessageCount();
        public readonly MessageCount MineralOutSyncBlockChain = new MessageCount();
        public readonly MessageCount MineralInBlockChainInventory = new MessageCount();
        public readonly MessageCount MineralOutBlockChainInventory = new MessageCount();

        public readonly MessageCount MineralInTrxInventory = new MessageCount();
        public readonly MessageCount MineralOutTrxInventory = new MessageCount();
        public readonly MessageCount MineralInTrxInventoryElement = new MessageCount();
        public readonly MessageCount MineralOutTrxInventoryElement = new MessageCount();

        public readonly MessageCount MineralInBlockInventory = new MessageCount();
        public readonly MessageCount MineralOutBlockInventory = new MessageCount();
        public readonly MessageCount MineralInBlockInventoryElement = new MessageCount();
        public readonly MessageCount MineralOutBlockInventoryElement = new MessageCount();

        public readonly MessageCount MineralInTrxFetchInvData = new MessageCount();
        public readonly MessageCount MineralOutTrxFetchInvData = new MessageCount();
        public readonly MessageCount MineralInTrxFetchInvDataElement = new MessageCount();
        public readonly MessageCount MineralOutTrxFetchInvDataElement = new MessageCount();

        public readonly MessageCount MineralInBlockFetchInvData = new MessageCount();
        public readonly MessageCount MineralOutBlockFetchInvData = new MessageCount();
        public readonly MessageCount MineralInBlockFetchInvDataElement = new MessageCount();
        public readonly MessageCount MineralOutBlockFetchInvDataElement = new MessageCount();


        public readonly MessageCount MineralInTrx = new MessageCount();
        public readonly MessageCount MineralOutTrx = new MessageCount();
        public readonly MessageCount MineralInTrxs = new MessageCount();
        public readonly MessageCount MineralOutTrxs = new MessageCount();
        public readonly MessageCount MineralInBlock = new MessageCount();
        public readonly MessageCount MineralOutBlock = new MessageCount();
        public readonly MessageCount MineralOutAdvBlock = new MessageCount();
        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void AddUdpInMessage(UdpMessageType type)
        {
            AddUdpMessage(type, true);
        }

        public void AddUdpOutMessage(UdpMessageType type)
        {
            AddUdpMessage(type, false);
        }

        public void AddTcpInMessage(Message message)
        {
            AddTcpMessage(message, true);
        }

        public void AddTcpOutMessage(Message message)
        {
            AddTcpMessage(message, false);
        }

        private void AddUdpMessage(UdpMessageType type, bool flag)
        {
            switch (type)
            {
                case UdpMessageType.DISCOVER_PING:
                    if (flag)
                        DiscoverInPing.Add();
                    else
                        DiscoverOutPing.Add();
                    break;
                case UdpMessageType.DISCOVER_PONG:
                    if (flag)
                        DiscoverInPong.Add();
                    else
                        DiscoverOutPong.Add();
                    break;
                case UdpMessageType.DISCOVER_FIND_NODE:
                    if (flag)
                        DiscoverInFindNode.Add();
                    else
                        DiscoverOutFindNode.Add();
                    break;
                case UdpMessageType.DISCOVER_NEIGHBORS:
                    if (flag)
                        DiscoverInNeighbours.Add();
                    else
                        DiscoverOutNeighbours.Add();
                    break;
                default:
                    break;
            }
        }
        
        private void AddTcpMessage(Message message, bool flag)
        {
            if (flag)
                MineralInMessage.Add();
            else
                MineralOutMessage.Add();

            switch (message.Type)
            {
                case MessageTypes.MsgType.P2P_HELLO:
                    if (flag)
                        P2pInHello.Add();
                    else
                        P2pOutHello.Add();
                    break;
                case MessageTypes.MsgType.P2P_PING:
                    if (flag)
                        P2pInPing.Add();
                    else
                        P2pOutPing.Add();
                    break;
                case MessageTypes.MsgType.P2P_PONG:
                    if (flag)
                        P2pInPong.Add();
                    else
                        P2pOutPong.Add();
                    break;
                case MessageTypes.MsgType.P2P_DISCONNECT:
                    if (flag)
                        P2pInDisconnect.Add();
                    else
                        P2pOutDisconnect.Add();
                    break;
                case MessageTypes.MsgType.SYNC_BLOCK_CHAIN:
                    if (flag)
                        MineralInSyncBlockChain.Add();
                    else
                        MineralOutSyncBlockChain.Add();
                    break;
                case MessageTypes.MsgType.BLOCK_CHAIN_INVENTORY:
                    if (flag)
                        MineralInBlockChainInventory.Add();
                    else
                        MineralOutBlockChainInventory.Add();
                    break;
                case MessageTypes.MsgType.INVENTORY:
                    InventoryMessage inventory_message = (InventoryMessage)message;
                    int inventory_count = inventory_message.Inventory.Ids.Count;
                    if (flag)
                    {
                        if (inventory_message.InventoryMessageType ==  MessageTypes.MsgType.TX)
                        {
                            MineralInTrxInventory.Add();
                            MineralInTrxInventoryElement.Add(inventory_count);
                        }
                        else
                        {
                            MineralInBlockInventory.Add();
                            MineralInBlockInventoryElement.Add(inventory_count);
                        }
                    }
                    else
                    {
                        if (inventory_message.InventoryMessageType == MessageTypes.MsgType.TX)
                        {
                            MineralOutTrxInventory.Add();
                            MineralOutTrxInventoryElement.Add(inventory_count);
                        }
                        else
                        {
                            MineralOutBlockInventory.Add();
                            MineralOutBlockInventoryElement.Add(inventory_count);
                        }
                    }
                    break;
                case MessageTypes.MsgType.FETCH_INV_DATA:
                    FetchInventoryDataMessage fetch_inventory_message = (FetchInventoryDataMessage)message;
                    int fetch_count = fetch_inventory_message.Inventory.Ids.Count;
                    if (flag)
                    {
                        if (fetch_inventory_message.InventoryMessageType == MessageTypes.MsgType.TX)
                        {
                            MineralInTrxFetchInvData.Add();
                            MineralInTrxFetchInvDataElement.Add(fetch_count);
                        }
                        else
                        {
                            MineralInBlockFetchInvData.Add();
                            MineralInBlockFetchInvDataElement.Add(fetch_count);
                        }
                    }
                    else
                    {
                        if (fetch_inventory_message.InventoryMessageType == MessageTypes.MsgType.TX)
                        {
                            MineralOutTrxFetchInvData.Add();
                            MineralOutTrxFetchInvDataElement.Add(fetch_count);
                        }
                        else
                        {
                            MineralOutBlockFetchInvData.Add();
                            MineralOutBlockFetchInvDataElement.Add(fetch_count);
                        }
                    }
                    break;
                case MessageTypes.MsgType.TXS:
                    TransactionsMessage transactionsMessage = (TransactionsMessage)message;
                    if (flag)
                    {
                        MineralInTrxs.Add();
                        MineralInTrx.Add(transactionsMessage.Transactions.Transactions_.Count);
                    }
                    else
                    {
                        MineralOutTrxs.Add();
                        MineralOutTrx.Add(transactionsMessage.Transactions.Transactions_.Count);
                    }
                    break;
                case MessageTypes.MsgType.TX:
                    if (flag)
                    {
                        MineralInMessage.Add();
                    }
                    else
                    {
                        MineralOutMessage.Add();
                    }
                    break;
                case MessageTypes.MsgType.BLOCK:
                    if (flag)
                    {
                        MineralInBlock.Add();
                    }
                    MineralOutBlock.Add();
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
