using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Capsule.Util
{
    public class ExchangeProcessor
    {
        #region Field
        private long supply = 0;
        #endregion


        #region Property
        #endregion


        #region Contructor
        public ExchangeProcessor(long supply)
        {
            this.supply = supply;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private long ExchangeToSupply(long balance, long quantity)
        {
            Logger.Debug("balance: " + balance);
            long new_balance = balance + quantity;
            Logger.Debug("balance + quantity: " + new_balance);

            double issued_supply = -supply * (1.0 - Math.Pow(1.0 + (double)quantity / new_balance, 0.0005));
            Logger.Debug("IssuedSupply: " + issued_supply);
            long output = (long)issued_supply;
            supply += output;

            return output;
        }

        private long ExchangeFromSupply(long balance, long supply_quantity)
        {
            supply -= supply_quantity;

            double exchangeBalance = balance * (Math.Pow(1.0 + (double)supply_quantity / supply, 2000.0) - 1.0);
            Logger.Debug("exchangeBalance: " + exchangeBalance);

            return (long)exchangeBalance;
        }
        #endregion


        #region External Method
        public long Exchange(long sell_token_balance, long buy_token_balance, long sell_token_quantity)
        {
            long relay = ExchangeToSupply(sell_token_balance, sell_token_quantity);

            return ExchangeFromSupply(buy_token_balance, relay);
        }
        #endregion
    }
}
