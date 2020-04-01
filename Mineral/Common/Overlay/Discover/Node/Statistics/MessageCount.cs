using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Overlay.Discover.Node.Statistics
{
    public class MessageCount
    {
        #region Field
        private static int SIZE = 60;
        private int[] message_count = new int[SIZE];
        private long total_count = 0;
        private long index_time = 0;
        private int index = 0;
        #endregion


        #region Property
        public long TotalCount => this.total_count;
        #endregion


        #region Constructor
        public MessageCount()
        {
            this.index_time = Helper.CurrentTimeMillis() / 1000;
            this.index = (int)(this.index_time % SIZE);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void Update()
        {
            long time = Helper.CurrentTimeMillis() / 1000;
            long gap = time - this.index_time;

            int k = gap > SIZE ? SIZE : (int)gap;
            if (k > 0)
            {
                for (int i = 1; i <= k; i++)
                {
                    this.message_count[(this.index + i) % SIZE] = 0;
                }
                this.index = (int)(time % SIZE);
                this.index_time = time;
            }
        }
        #endregion


        #region External Method
        public void Add()
        {
            Update();
            this.message_count[this.index]++;
            this.total_count++;
        }

        public void Add(int count)
        {
            Update();
            this.message_count[this.index] += count;
            this.total_count += count;
        }

        public int GetCount(int interval)
        {
            if (interval > SIZE)
            {
                Logger.Warning(string.Format("Param interval({0}) is gt SIZE({1})", interval, SIZE));
                return 0;
            }

            Update();
            int count = 0;
            for (int i = 0; i < interval; i++)
            {
                count += this.message_count[(SIZE + this.index - i) % SIZE];
            }

            return count;
        }

        public void Reset()
        {
            this.total_count = 0;
        }

        public override string ToString()
        {
            return this.total_count.ToString();
        }
        #endregion
    }
}
