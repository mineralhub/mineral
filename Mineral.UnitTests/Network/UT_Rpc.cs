using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Network.RPC;

namespace Mineral.UnitTests.Network
{
    [TestClass]
    public class UT_Rpc
    {
        RpcServer _rpcServer;

        [TestInitialize]
        public void TestSetup()
        {
            _rpcServer = new RpcServer(null);
            _rpcServer.Start(0);
        }

        [TestMethod]
        public void GetBlock()
        {
        }

        [TestMethod]
        public void GetTransaction()
        {
        }
    }
}
