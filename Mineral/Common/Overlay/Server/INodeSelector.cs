using Mineral.Common.Overlay.Discover.Node;

namespace Mineral.Common.Overlay.Server
{
    public interface INodeSelector
    {
        boolean test(NodeHandler handler);
    }
}