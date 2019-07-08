using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mineral.Common.Utils;
using Mineral.Core.Net.Messages;
using Mineral.Core.Net.Peer;
using static Mineral.Core.Capsule.BlockCapsule;
using static Protocol.Inventory.Types;

namespace Mineral.Core.Net.Service
{
    public class InventorySender
    {
        #region Field
        private Dictionary<PeerConnection, Dictionary<InventoryType, LinkedList<SHA256Hash>>> send = new Dictionary<PeerConnection, Dictionary<InventoryType, LinkedList<SHA256Hash>>>();
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Add(KeyValuePair<SHA256Hash, InventoryType> id, PeerConnection peer)
        {
            if (send.ContainsKey(peer) && !send[peer].ContainsKey(id.Value))
            {
                send[peer].Add(id.Value, new LinkedList<SHA256Hash>());
            }
            else if (!send.ContainsKey(peer))
            {
                send.Add(peer, new Dictionary<InventoryType, LinkedList<SHA256Hash>>());
                send[peer].Add(id.Value, new LinkedList<SHA256Hash>());
            }
            send[peer][id.Value].AddLast(id.Key);
        }

        public void Add(Item id, PeerConnection peer)
        {
            if (send.ContainsKey(peer) && !send[peer].ContainsKey(id.Type))
            {
                send[peer].Add(id.Type, new LinkedList<SHA256Hash>());
            }
            else if (!send.ContainsKey(peer))
            {
                send.Add(peer, new Dictionary<InventoryType, LinkedList<SHA256Hash>>());
                send[peer].Add(id.Type, new LinkedList<SHA256Hash>());
            }
            send[peer][id.Type].AddLast(id.Hash);
        }

        public int GetSize(PeerConnection peer)
        {
            if (send.ContainsKey(peer))
            {
                return send[peer].Values.Select(hashes => hashes.Count).Sum();
            }
            return 0;
        }

        public void SendInventory()
        {
            foreach (var s in this.send)
            {
                foreach (var id in s.Value)
                {
                    if (id.Key.Equals(InventoryType.Trx) && s.Key.IsFastForwardPeer)
                    {
                        return;
                    }

                    if (id.Key.Equals(InventoryType.Block))
                    {
                        id.Value.OrderBy(hash => new BlockId(hash).Num);
                    }

                    s.Key.SendMessage(new InventoryMessage(id.Value.ToList(), id.Key));
                }
            }
        }

        public void SendFetch()
        {
            foreach (var s in this.send)
            {
                foreach (var id in s.Value)
                {
                    if (id.Key.Equals(InventoryType.Block))
                    {
                        id.Value.OrderBy(hash => new BlockId(hash).Num);
                    }

                    s.Key.SendMessage(new FetchInventoryDataMessage(id.Value.ToList(), id.Key));
                }
            }
        }

        public void Clear()
        {
            this.send.Clear();
        }
        #endregion
    }
}
