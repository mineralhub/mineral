using System;
using System.Collections.Generic;
using System.Text;

namespace Sky.Core
{
    public class TransactionResult
    {
        public Fixed8 Amount { get; private set; }

        public TransactionResult(Fixed8 amount)
        {
            Amount = amount;
        }
    }
}
