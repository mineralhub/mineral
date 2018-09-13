using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sky.Core;
using Sky.Wallets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sky.UnitTests
{
    [TestClass]
    public class UT_Transaction
    {
        
        WalletAccount _from = new WalletAccount(Encoding.Default.GetBytes("0"));
        WalletAccount _to = new WalletAccount(Encoding.Default.GetBytes("1"));

        TransferTransaction _transfer;
        RewardTransaction _reward;
        VoteTransaction _vote;
        OtherSignTransaction _otherSign;
        SignTransaction _sign;
        RegisterDelegateTransaction _register;

        Transaction _transaction;

        [TestInitialize]
        public void TestSetup()
        {
            _transfer = new TransferTransaction
            {
                From = _from.AddressHash,
                To = new Dictionary<UInt160, Fixed8> { {_to.AddressHash, Fixed8.One } }
            };

            _reward = new RewardTransaction
            {
                From = _from.AddressHash,
                Reward = Config.BlockReward
            };

            _vote = new VoteTransaction
            {
                From = _from.AddressHash,
                Votes = new Dictionary<UInt160, Fixed8> { { _from.AddressHash, Fixed8.One } }
            };

            _otherSign = new OtherSignTransaction
            {
                From = _from.AddressHash,
                To = new Dictionary<UInt160, Fixed8> { { _to.AddressHash, Fixed8.One } },
                Others = new HashSet<string> { _from.Address, _to.Address },
                ValidBlockHeight = 10
            };

            _sign = new SignTransaction
            {
                From = _from.AddressHash,
                SignTxHash = new UInt256()
            };

            _register = new RegisterDelegateTransaction
            {
                From = _from.AddressHash,
                Name = Encoding.Default.GetBytes("delegate")
            };

            _transaction = new Transaction
            {
                Version = 0,
                Type = eTransactionType.TransferTransaction,
                Timestamp = DateTime.UtcNow.ToTimestamp(),
                Data = _transfer,
            };
            _transaction.Sign(_from.Key);
        }

        [TestMethod]
        public void Sign()
        {
            _transaction.VerifySignature().Should().Be(true);
        }

        [TestMethod]
        public void GetSize()
        {
            // txbase : 8 + 20
            int txbase = 28;
            int transferTxSize = 0;
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                _transfer.Serialize(bw);
                ms.Flush();
                ms.ToArray().Length.Should().Be(txbase + _transfer.To.GetSize()); // dynamic
                transferTxSize = ms.ToArray().Length;
            }

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                _reward.Serialize(bw);
                ms.Flush();
                ms.ToArray().Length.Should().Be(txbase + 8); // 36
            }

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                _vote.Serialize(bw);
                ms.Flush();
                ms.ToArray().Length.Should().Be(txbase + _vote.Votes.GetSize()); // dynamic
            }

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                _otherSign.Serialize(bw);
                ms.Flush();
                ms.ToArray().Length.Should().Be(txbase + _otherSign.To.GetSize() + _otherSign.Others.GetSize() + 4); // dynamic
            }

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                _sign.Serialize(bw);
                ms.Flush();
                ms.ToArray().Length.Should().Be(txbase + 32); // 60
            }

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                _register.Serialize(bw);
                ms.Flush();
                ms.ToArray().Length.Should().Be(txbase + _register.Name.GetSize()); // dynamic
            }

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                // 2 + 2 + 4 + transfer
                _transaction.SerializeUnsigned(bw);
                ms.Flush();
                ms.ToArray().Length.Should().Be(8 + transferTxSize);
            }
        }
    }
}
