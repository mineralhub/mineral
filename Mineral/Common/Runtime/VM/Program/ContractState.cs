using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Runtime.VM.Program.Invoke;
using Mineral.Common.Runtime.VM.Program.Listener;
using Mineral.Common.Storage;
using Mineral.Core.Capsule;
using Mineral.Core.Database;
using Protocol;

namespace Mineral.Common.Runtime.VM.Program
{
    public class ContractState : IDeposit, IProgramListenerAware
    {
        #region Field
        private Deposit deposit = null;
        private readonly DataWord address = null;
        private IProgramListener program_listener = null;
        #endregion


        #region Property
        public DataBaseManager DBManager
        {
            get { return this.deposit?.DBManager; }
        }
        #endregion


        #region Contructor
        public ContractState(IProgramInvoke invoke)
        {
            this.address = invoke.GetContractAddress();
            this.deposit = invoke.GetDeposit();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private bool CanListenTrace(byte[] address)
        {
            return (this.program_listener != null) && this.address.Equals(new DataWord(address));
        }
        #endregion


        #region External Method
        public void SetProgramListener(IProgramListener listener)
        {
            this.program_listener = listener;
        }

        public long AddBalance(byte[] address, long value)
        {
            return this.deposit.AddBalance(address, value);
        }

        public long AddTokenBalance(byte[] address, byte[] token_id, long value)
        {
            return this.deposit.AddTokenBalance(address, token_id, value);
        }

        public void Commit()
        {
            this.deposit.Commit();
        }

        public AccountCapsule CreateAccount(byte[] address, AccountType type)
        {
            return this.deposit.CreateAccount(address, type);
        }

        public AccountCapsule CreateAccount(byte[] address, string account_name, AccountType type)
        {
            return this.deposit.CreateAccount(address, account_name, type);
        }

        public void CreateContract(byte[] address, ContractCapsule contract)
        {
            this.deposit.CreateContract(address, contract);
        }

        public void DeleteContract(byte[] address)
        {
            this.deposit.DeleteContract(address);
        }

        public AccountCapsule GetAccount(byte[] address)
        {
            return this.deposit.GetAccount(address);
        }

        public AssetIssueCapsule GetAssetIssue(byte[] token_id)
        {
            return this.deposit.GetAssetIssue(token_id);
        }

        public long GetBalance(byte[] address)
        {
            return this.deposit.GetBalance(address);
        }

        public byte[] GetBlackHoleAddress()
        {
            return this.deposit.GetBlackHoleAddress();
        }

        public BlockCapsule GetBlock(byte[] hash)
        {
            return this.deposit.GetBlock(hash);
        }

        public byte[] GetCode(byte[] address)
        {
            return this.deposit.GetCode(address);
        }

        public ContractCapsule GetContract(byte[] address)
        {
            return this.deposit.GetContract(address);
        }

        public BytesCapsule GetDynamic(byte[] dynamic_key)
        {
            return this.deposit.GetDynamic(dynamic_key);
        }

        public long GetLatestProposalNum()
        {
            return this.deposit.GetLatestProposalNum();
        }

        public long GetMaintenanceTimeInterval()
        {
            return this.deposit.GetMaintenanceTimeInterval();
        }

        public long GetNextMaintenanceTime()
        {
            return this.deposit.GetNextMaintenanceTime();
        }

        public ProposalCapsule GetProposalCapsule(byte[] id)
        {
            return this.deposit.GetProposalCapsule(id);
        }

        public Storage GetStorage(byte[] address)
        {
            return this.deposit.GetStorage(address);
        }

        public DataWord GetStorageValue(byte[] address, DataWord key)
        {
            return this.deposit.GetStorageValue(address, key);
        }

        public long GetTokenBalance(byte[] address, byte[] token_id)
        {
            return this.deposit.GetTokenBalance(address, token_id);
        }

        public TransactionCapsule GetTransaction(byte[] hash)
        {
            return this.deposit.GetTransaction(hash);
        }

        public VotesCapsule GetVotesCapsule(byte[] address)
        {
            return this.deposit.GetVotesCapsule(address);
        }

        public WitnessCapsule GetWitness(byte[] address)
        {
            return this.deposit.GetWitness(address);
        }

        public long GetWitnessAllowanceFrozenTime()
        {
            return this.deposit.GetWitnessAllowanceFrozenTime();
        }

        public IDeposit NewDepositChild()
        {
            return this.deposit.NewDepositChild();
        }

        public void PutAccount(Common.Storage.Key key, Value value)
        {
            this.deposit.PutAccount(key, value);
        }

        public void PutAccountValue(byte[] address, AccountCapsule account)
        {
            this.deposit.PutAccountValue(address, account);
        }

        public void PutBlock(Common.Storage.Key key, Value value)
        {
            this.deposit.PutBlock(key, value);
        }

        public void PutCode(Common.Storage.Key key, Value value)
        {
            this.deposit.PutCode(key, value);
        }

        public void PutContract(Common.Storage.Key key, Value value)
        {
            this.deposit.PutContract(key, value);
        }

        public void PutDynamicProperties(Common.Storage.Key key, Value value)
        {
            this.deposit.PutDynamicProperties(key, value);
        }

        public void PutDynamicPropertiesWithLatestProposalNum(long num)
        {
            this.deposit.PutDynamicPropertiesWithLatestProposalNum(num);
        }

        public void PutProposal(Common.Storage.Key key, Value value)
        {
            this.deposit.PutProposal(key, value);
        }

        public void PutProposalValue(byte[] address, ProposalCapsule proposal)
        {
            this.deposit.PutProposalValue(address, proposal);
        }

        public void PutStorage(Common.Storage.Key key, Storage cache)
        {
            this.deposit.PutStorage(key, cache);
        }

        public void PutStorageValue(byte[] address, DataWord key, DataWord value)
        {
            if (CanListenTrace(address))
                this.program_listener.OnStoragePut(key, value);
            else
                this.deposit.PutStorageValue(address, key, value);
        }

        public void PutTransaction(Common.Storage.Key key, Value value)
        {
            this.deposit.PutTransaction(key, value);
        }

        public void PutVotes(Common.Storage.Key key, Value value)
        {
            this.deposit.PutVotes(key, value);
        }

        public void PutVoteValue(byte[] address, VotesCapsule votes)
        {
            this.deposit.PutVoteValue(address, votes);
        }

        public void PutWitness(Common.Storage.Key key, Value value)
        {
            this.deposit.PutWitness(key, value);
        }

        public void SaveCode(byte[] address, byte[] code)
        {
            this.deposit.SaveCode(address, code);
        }

        public void SetParent(Deposit deposit)
        {
            this.deposit.SetParent(deposit);
        }

        public void UpdateContract(byte[] address, ContractCapsule contract)
        {
            this.deposit.UpdateContract(address, contract);
        }
        #endregion
    }
}
