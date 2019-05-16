using Mineral.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core2.DPos
{
    public class Delegator
    {
        private string _name;
        private UInt256 _txid;

        public Delegator(string name, UInt256 txid)
        {
            _name = name;
            _txid = txid;
        }
    }

    public class Delegate
    {
        private Dictionary<string, Delegator> _delegators = new Dictionary<string, Delegator>();

        public List<Delegator> GenerateDelegateList(long height)
        {
            return new List<Delegator>();
            //_delegators.Clear();
            //int round = CalcRound(height);
            //return _delegators;
        }

        public bool AddDelegate(string name, UInt256 txHash)
        {
            if (_delegators.ContainsKey(name))
                return false;

            _delegators.Add(name, new Delegator(name, txHash));
            return true;
        }

        public bool Forge()
        {
            return true;
        }

        private int CalcRound(long height)
        {
            return (int)Math.Ceiling((double)(height / Constants.ActiveDelegates));
        }
    }
}
