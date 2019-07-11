using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Overlay.Discover.Node;

namespace Mineral.Common.Overlay.Discover
{
    public interface IDiscoverListener
    {
        void NodeAppeared(NodeHandler handler);
        void NodeDisappeared(NodeHandler handler);
    }
}
