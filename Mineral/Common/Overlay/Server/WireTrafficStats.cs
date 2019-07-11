using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Mineral.Common.Overlay.Server
{
    public class WireTrafficStats
    {
        #region Field
        private TrafficStatHandler tcp = new TrafficStatHandler();
        private TrafficStatHandler udp = new TrafficStatHandler();
        #endregion


        #region Property
        public TrafficStatHandler TCP
        {
            get { return this.tcp; }
        }

        public TrafficStatHandler UDP
        {
            get { return this.udp; }
        }
        #endregion


        #region Contructor
        public WireTrafficStats()
        {
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
