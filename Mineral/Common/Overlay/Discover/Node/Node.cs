using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Exception;
using Mineral.Cryptography;
using Mineral.Utils;

namespace Mineral.Common.Overlay.Discover.Node
{
    public class Node
    {
        #region Field
        #endregion


        #region Property
        public byte[] Id { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool IsDiscoveryNode {get; set;}
        public int Reputation { get; set; }
        #endregion


        #region Constructor
        public Node(string enode_url)
        {
            try
            {
                Uri uri = new Uri(enode_url);
                if (!uri.Scheme.Equals("enode"))
                    throw new ConfigrationException("Exception URL in the format enode://PUBLIC@HOST:PORT");

                Id = uri.UserInfo.HexToBytes();
                Host = uri.Host;
                Port = uri.Port;
            }
            catch
            {
                throw new ConfigrationException("Exception URL in the format enode://PUBLIC@HOST:PORT");
            }
        }

        public Node(byte[] id, string host, int port)
        {
            if (id != null)
            {
                Id = new byte[id.Length];
                Array.Copy(id, 0, Id, 0, id.Length);
            }

            Host = host;
            Port = port;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static Node InstanceOf(string address)
        {
            try
            {
                Uri uri = new Uri(address);
                if (uri.Scheme.Equals("enode"))
                {
                    return new Node(address);
                }
            }
            catch (System.Exception e)
            {
                Logger.Debug(e.Message);
            }

            byte[] privatekey = Hash.SHA256(address.ToBytes());
            Cryptography.ECKey key = Cryptography.ECKey.FromPrivateKey(privatekey);

            byte[] node_id = new byte[privatekey.Length - 1];
            Array.Copy(key.PublicKey, 1, node_id, 0, node_id.Length);

            string id = node_id.ToHexString();
            Node node = new Node("enode://" + id + "@" + address);
            node.IsDiscoveryNode = true;

            return node;
        }

        public string GetEncodeURL()
        {
            return new StringBuilder()
                .Append("enode://")
                .Append(Id.ToHexString())
                .Append("@")
                .Append(Host)
                .Append(":")
                .Append(Port).ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj == this)
            {
                return true;
            }

            if (obj.GetType() == GetType())
            {
                Node n = ((Node)obj);

                return string.Equals(Id.ToString(), n.Id.ToString())
                    && string.Equals(Host, n.Host)
                    && string.Equals(Port, n.Port);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override string ToString()
        {
            return "Node{" + " host='" + Host + '\'' + ", port=" + Port + ", id=" + Id.ToHexString() + '}';
        }
        #endregion
    }
}
