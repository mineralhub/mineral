using System.Collections.Generic;

namespace Sky.Core
{
    public class RewardTransaction : TransactionBase
    {
        public RewardTransaction(List<TransactionInput> inputs, List<TransactionOutput> outputs, List<MakerSignature> signatures)
            : base(inputs, outputs, signatures)
        {
        }

        public override void CalcFee()
        {
            Fee = Fixed8.Zero;
        }

        public override bool Verify()
        {
            // zero input
            if (0 < Inputs.Count)
                return false;
            // single output
            if (1 < Outputs.Count)
                return false;
            // block reward
            if (Outputs[0].Value != Config.BlockReward)
                return false;
            return true;
        }
    }
}
