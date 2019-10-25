using System;
using Mineral.Common.Runtime.VM;
using Mineral.Common.Runtime.VM.Program;
using Mineral.Core.Capsule;
using Mineral.Core.Database;
using static Mineral.Core.Config.Arguments.Account;

namespace Mineral.Common.Storage
{
    using VMStorage = Runtime.VM.Program.Storage;

    public interface IDeposit
    {
        DatabaseManager DBManager { get; }

        AccountCapsule CreateAccount(byte[] address, Protocol.AccountType type);
        AccountCapsule CreateAccount(byte[] address, string account_name, Protocol.AccountType type);
        AccountCapsule GetAccount(byte[] address);
        WitnessCapsule GetWitness(byte[] address);
        VotesCapsule GetVotesCapsule(byte[] address);
        ProposalCapsule GetProposalCapsule(byte[] id);
        BytesCapsule GetDynamic(byte[] dynamic_key);
        ContractCapsule GetContract(byte[] address);

        void CreateContract(byte[] address, ContractCapsule contract);
        void UpdateContract(byte[] address, ContractCapsule contract);
        void DeleteContract(byte[] address);

        void SaveCode(byte[] address, byte[] code);
        byte[] GetCode(byte[] address);

        void PutStorageValue(byte[] address, DataWord key, DataWord value);
        DataWord GetStorageValue(byte[] address, DataWord key);

        Mineral.Common.Runtime.VM.Program.Storage GetStorage(byte[] address);

        long GetBalance(byte[] address);
        long AddBalance(byte[] address, long value);
        long AddTokenBalance(byte[] address, byte[] token_id, long value);

        IDeposit NewDepositChild();

        void PutBlock(Key key, Value value);
        void PutTransaction(Key key, Value value);
        void PutAccount(Key key, Value value);
        void PutWitness(Key key, Value value);
        void PutCode(Key key, Value value);
        void PutContract(Key key, Value value);
        void PutStorage(Key key, VMStorage cache);
        void PutVotes(Key key, Value value);
        void PutProposal(Key key, Value value);
        void PutDynamicProperties(Key key, Value value);
        void PutAccountValue(byte[] address, AccountCapsule account);
        void PutVoteValue(byte[] address, VotesCapsule votes);
        void PutProposalValue(byte[] address, ProposalCapsule proposal);
        void PutDynamicPropertiesWithLatestProposalNum(long num);

        long GetLatestProposalNum();
        long GetWitnessAllowanceFrozenTime();
        long GetMaintenanceTimeInterval();
        long GetNextMaintenanceTime();
        long GetTokenBalance(byte[] address, byte[] token_id);

        byte[] GetBlackHoleAddress();
        AssetIssueCapsule GetAssetIssue(byte[] token_id);
        TransactionCapsule GetTransaction(byte[] hash);
        BlockCapsule GetBlock(byte[] hash);

        void SetParent(Deposit deposit);
        void Commit();
    }
}
