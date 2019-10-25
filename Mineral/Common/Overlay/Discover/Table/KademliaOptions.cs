using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Overlay.Discover.Table
{
    public class KademliaOptions
    {
        public static readonly int BUCKET_SIZE = 16;
        public static readonly int ALPHA = 3;
        public static readonly int BINS = 256;
        public static readonly int MAX_STEPS = 8;

        public static readonly long REQ_TIMEOUT = 300;
        public static readonly long BUCKET_REFRESH = 7200;
        public static readonly long DISCOVER_CYCLE = 30;
    }
}
