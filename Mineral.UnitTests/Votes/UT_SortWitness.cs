using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Google.Protobuf;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Core.Capsule;
using Mineral.Core.Witness;

namespace Mineral.UnitTests.Votes
{
    [TestClass]
    public class UT_SortWitness
    {
        private WitnessCapsule witness1 = new WitnessCapsule(ByteString.CopyFrom(Core.Wallet.Base58ToAddress("MJ2hJegzrjDBZEUBmPhbCUqg9jjhYSC7q9")), 10000000, "test1");
        private WitnessCapsule witness2 = new WitnessCapsule(ByteString.CopyFrom(Core.Wallet.Base58ToAddress("MFBvcsYU5NPTAHMJvtYeJEezwFsLduBrTs")), 20000000, "test2");
        private WitnessCapsule witness3 = new WitnessCapsule(ByteString.CopyFrom(Core.Wallet.Base58ToAddress("MLtaz8vSsfSs1WTErMbWQK8wpyGPXeuUih")), 30000000, "test3");
        private WitnessCapsule witness4 = new WitnessCapsule(ByteString.CopyFrom(Core.Wallet.Base58ToAddress("MUhMn46Qcortx1Wae41xezFjaFf3wc73b9")), 30000000, "test4");

        private string[] result_address = new string[]
        {
            "MLtaz8vSsfSs1WTErMbWQK8wpyGPXeuUih",
            "MUhMn46Qcortx1Wae41xezFjaFf3wc73b9",
            "MFBvcsYU5NPTAHMJvtYeJEezwFsLduBrTs",
            "MJ2hJegzrjDBZEUBmPhbCUqg9jjhYSC7q9"
        };

        private List<WitnessCapsule> items = new List<WitnessCapsule>();

        [TestInitialize]
        public void TestSetup()
        {
            items.Add(witness1);
            items.Add(witness2);
            items.Add(witness3);
            items.Add(witness4);
        }

        [TestCleanup]
        public void CleanUp()
        {
        }

        [TestMethod]
        public void SortWitnessComparer()
        {
            List<WitnessCapsule> result = this.items.OrderBy(x => x, new WitnessSortComparer()).ToList();

            for (int index = 0; index < 4; index++)
            {
                Core.Wallet.AddressToBase58(result[index].Address.ToByteArray()).Should().Be(this.result_address[index]);
            }
        }
    }
}
