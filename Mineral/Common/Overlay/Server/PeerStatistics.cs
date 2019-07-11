/*
 * Copyright (c) [2016] [ <ether.camp> ]
 * This file is part of the ethereumJ library.
 *
 * The ethereumJ library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * The ethereumJ library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with the ethereumJ library. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Overlay.Server
{
    public class PeerStatistics
    {
        #region Field
        private double average_latency = 0;
        private long ping_count = 0;
        #endregion


        #region Property
        public double AverageLatency
        {
            get { return this.average_latency; }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Pong(long stamp)
        {
            long latency = Helper.CurrentTimeMillis() - stamp;
            this.average_latency = ((this.average_latency * this.ping_count) + latency) / ++this.ping_count;
        }
        #endregion
    }
}
