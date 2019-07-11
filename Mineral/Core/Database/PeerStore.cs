using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Overlay.Discover.Node;

namespace Mineral.Core.Database
{
    public class PeerStore : MineralDatabase<HashSet<Node>>
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public PeerStore() : base ("peers") { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override bool Contains(byte[] key)
        {
            return this.db_source.GetData(key) != null;
        }

        public override HashSet<Node> Get(byte[] key)
        {
            HashSet<Node> nodes = new HashSet<Node>();
            byte[] value = this.db_source.GetData(key);
            if (value != null)
            {
                string data = Encoding.UTF8.GetString(value);
                string[] split = data.Split("||");
                
                foreach (string s in split)
                {
                    Node node = null;
                    int rept = 0;
                    int pos = s.IndexOf("&");
                    if (pos > 0)
                    {
                        node = new Node(s.Substring(0, pos));
                        try
                        {
                            rept = int.Parse(s.Substring(pos + 1, s.Length));
                        }
                        catch
                        {
                            rept = 0;
                        }
                    }
                    else
                    {
                        node = new Node(s);
                        rept = 0;
                    }

                    node.Reputation = rept;
                    nodes.Add(node);
                }
            }

            return nodes;
        }

        public override void Put(byte[] key, HashSet<Node> value)
        {
            StringBuilder sb = new StringBuilder();
            
            foreach (Node node in value)
            {
                sb.Append(node.GetEncodeURL()).Append("&").Append(node.Reputation).Append("||");
                this.db_source.PutData(key, Encoding.UTF8.GetBytes(sb.ToString()));
            }
        }

        public override void Delete(byte[] key)
        {
            this.db_source.DeleteData(key);
        }
        #endregion
    }
}
