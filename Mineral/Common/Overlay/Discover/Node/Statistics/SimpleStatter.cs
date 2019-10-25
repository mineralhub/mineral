using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Mineral.Common.Overlay.Discover.Node.Statistics
{
    public class SimpleStatter
    {
        #region Field
        private readonly string name = "";
        private double last = 0;
        private double sum = 0;
        private int count = 0;
        #endregion


        #region Property
        #endregion


        #region Constructor
        public SimpleStatter(string name)
        {
            this.name = name;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Add(double value)
        {
            this.last = value;
            this.sum += value;
            Interlocked.Increment(ref count);
        }

        public double GetLast()
        {
            return this.last;
        }

        public double GetSum()
        {
            return this.sum;
        }

        public int GetCount()
        {
            return this.count;
        }

        public string GetName()
        {
            return this.name;
        }

        public double GetAverage()
        {
            return this.count == 0 ? 0 : this.sum / count;
        }
        #endregion
    }
}
