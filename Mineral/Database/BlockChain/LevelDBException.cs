using System;
using System.Collections.Generic;

namespace Mineral.Database.BlockChain
{
    public class LevelDBException : Exception
    {
        public LevelDBException(string message)
            : base(message)
        {
        }
    }
}
