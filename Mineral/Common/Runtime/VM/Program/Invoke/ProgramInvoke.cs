using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mineral.Common.Storage;
using Mineral.Core.Capsule;
using Org.BouncyCastle.Math;

namespace Mineral.Common.Runtime.VM.Program.Invoke
{
    public class ProgramInvoke : IProgramInvoke
    {
        #region Field
        private static readonly BigInteger MAX_MSG_DATA = BigInteger.ValueOf(int.MaxValue);

        private readonly DataWord address = null;
        private readonly DataWord origin = null;
        private readonly DataWord caller = null;
        private readonly DataWord balance = null;
        private readonly DataWord call_value = null;
        private readonly DataWord token_value = null;
        private readonly DataWord token_id = null;

        private byte[] msg = null;
        private long vm_start = 0;
        private long vm_should_end = 0;
        private long energy_limit = 0;

        private readonly DataWord prev_hash = null;
        private readonly DataWord coinbase = null;
        private readonly DataWord timestamp = null;
        private readonly DataWord number = null;

        private IDeposit deposit = null;
        private bool by_transaction = true;
        private bool is_testing_suite = false;
        private int call_deep = 0;
        private bool is_static_call = false;
        #endregion


        #region Property
        public bool IsStaticCall
        {
            get { return this.is_static_call; }
            set { this.is_static_call = value; }
        }

        public bool IsTestingSuite
        {
            get { return this.is_testing_suite; }
        }

        public DataWord Balance
        {
            get { return this.balance; }
        }

        public int CallDeep
        {
            get { return this.call_deep; }
        }

        public DataWord CallerAddress
        {
            get { return this.caller; }
        }

        public DataWord CallValue
        {
            get { return this.call_value; }
        }

        public DataWord Coinbase
        {
            get { return this.coinbase; }
        }

        public DataWord ContractAddress
        {
            get { return this.address; }
        }

        public IDeposit Deposit
        {
            get { return this.deposit; }
        }

        public DataWord Difficulty
        {
            get { return new DataWord(0); }
        }

        public long EnergyLimit
        {
            get { return this.energy_limit; }
        }

        public DataWord Number
        {
            get { return this.number; }
        }

        public DataWord OriginAddress
        {
            get { return this.origin; }
        }

        public DataWord PrevHash
        {
            get { return this.prev_hash; }
        }

        public DataWord Timestamp
        {
            get { return this.timestamp; }
        }

        public DataWord TokenId
        {
            get { return this.token_id; }
        }

        public DataWord TokenValue
        {
            get { return this.token_value; }
        }

        public long VMShouldEndInUs
        {
            get { return this.vm_should_end; }
        }

        public long VMStartInUs
        {
            get { return this.vm_start; }
        }

        public DataWord DataSize => throw new NotImplementedException();
        #endregion


        #region Contructor
        public ProgramInvoke(DataWord address, DataWord origin, DataWord caller, DataWord balance,
                             DataWord call_value, DataWord token_value, DataWord token_id, byte[] msg,
                             DataWord last_hash, DataWord coinbase, DataWord timestamp, DataWord number,
                             DataWord difficulty,
                             IDeposit deposit, int call_deep, bool is_static_call, bool is_testing_suite,
                             long vm_start, long vm_should_end, long energy_limit)
        {
            this.address = address;
            this.origin = origin;
            this.caller = caller;
            this.balance = balance;
            this.call_value = call_value;
            this.token_value = token_value;
            this.token_id = token_id;
            if (msg != null && msg.Length > 0)
            {
                Array.Copy(msg, 0, this.msg, 0, msg.Length);
            }

            this.prev_hash = last_hash;
            this.coinbase = coinbase;
            this.timestamp = timestamp;
            this.number = number;
            this.call_deep = call_deep;

            this.deposit = deposit;
            this.by_transaction = false;
            this.is_static_call = is_static_call;
            this.is_testing_suite = is_testing_suite;
            this.vm_start = vm_start;
            this.vm_should_end = vm_should_end;
            this.energy_limit = energy_limit;

        }

        public ProgramInvoke(byte[] address, byte[] origin, byte[] caller, long balance,
                             long call_value, long token_value, long token_id, byte[] msg,
                             byte[] last_hash, byte[] coinbase, long timestamp, long number, Deposit deposit,
                             long vm_start, long vm_should_end, bool is_testing_suite, long energy_limit)
            : this(address, origin, caller, balance,
                   call_value, token_value, token_id, msg, last_hash, coinbase,
                   timestamp, number, deposit, vm_start, vm_should_end, energy_limit)
        {
            this.is_testing_suite = is_testing_suite;
        }

        public ProgramInvoke(byte[] address, byte[] origin, byte[] caller, long balance,
                             long call_value, long token_value, long token_id, byte[] msg,
                             byte[] last_hash, byte[] coinbase, long timestamp,
                             long number, IDeposit deposit, long vm_start, long vm_should_end, long energy_limit)
        {

            this.address = new DataWord(address);
            this.origin = new DataWord(origin);
            this.caller = new DataWord(caller);
            this.balance = new DataWord(balance);
            this.call_value = new DataWord(call_value);
            this.token_value = new DataWord(token_value);
            this.token_id = new DataWord(token_id);
            if (msg != null && msg.Length > 0)
            {
                Array.Copy(msg, 0, this.msg, 0, msg.Length);
            }

            this.prev_hash = new DataWord(last_hash);
            this.coinbase = new DataWord(coinbase);
            this.timestamp = new DataWord(timestamp);
            this.number = new DataWord(number);
            this.deposit = deposit;

            this.vm_start = vm_start;
            this.vm_should_end = vm_should_end;
            this.energy_limit = energy_limit;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public BlockCapsule GetBlockByNum(int index)
        {
            try
            {
                return this.deposit.DBManager.GetBlockByNum(index);
            }
            catch (System.Exception e)
            {
                throw new ArgumentException("cannot find block num");
            }
        }

        public byte[] GetDataCopy(DataWord offset_data, DataWord length_data)
        {
            int offset = offset_data.ToIntSafety();
            int length = length_data.ToIntSafety();

            byte[] data = new byte[length];

            if (msg == null)
            {
                return data;
            }
            if (offset > msg.Length)
            {
                return data;
            }
            if (offset + length > msg.Length)
            {
                length = msg.Length - offset;
            }

            Array.Copy(msg, offset, data, 0, length);

            return data;
        }

        public DataWord GetDataSize()
        {
            if (msg == null || msg.Length == 0)
            {
                return DataWord.ZERO;
            }
            int size = msg.Length;

            return new DataWord(size);
        }

        public DataWord GetDataValue(DataWord index_data)
        {
            BigInteger temp_index = index_data.ToBigInteger();
            int index = temp_index.IntValue;
            int size = 32; // maximum datavalue size

            if (msg == null || index >= msg.Length
                || temp_index.CompareTo(MAX_MSG_DATA) > 0)
            {
                return new DataWord();
            }
            if (index + size > msg.Length)
            {
                size = msg.Length - index;
            }

            byte[] data = new byte[32];
            Array.Copy(msg, index, data, 0, size);

            return new DataWord(data);
        }

        public override int GetHashCode()
        {
            return (this.is_testing_suite.GetHashCode()
                    + this.by_transaction.GetHashCode()
                    + this.address.GetHashCode()
                    + this.balance.GetHashCode()
                    + this.call_value.GetHashCode()
                    + this.caller.GetHashCode()
                    + this.coinbase.GetHashCode()
                    + this.msg.GetHashCode()
                    + this.number.GetHashCode()
                    + this.origin.GetHashCode()
                    + this.prev_hash.GetHashCode()
                    + this.deposit.GetHashCode()
                    + this.timestamp.GetHashCode()
                ).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            ProgramInvoke target = (ProgramInvoke)obj;

            if (this.is_testing_suite != target.is_testing_suite)
            {
                return false;
            }
            if (this.by_transaction != target.by_transaction)
            {
                return false;
            }
            if (this.address != null ? !this.address.Equals(target.address) : target.address != null)
            {
                return false;
            }
            if (this.balance != null ? !this.balance.Equals(target.balance) : target.balance != null)
            {
                return false;
            }
            if (this.call_value != null ? !this.call_value.Equals(target.call_value) : target.call_value != null)
            {
                return false;
            }
            if (this.caller != null ? !this.caller.Equals(target.caller) : target.caller != null)
            {
                return false;
            }
            if (this.coinbase != null ? !this.coinbase.Equals(target.coinbase) : target.coinbase != null)
            {
                return false;
            }
            if (!this.msg.SequenceEqual(target.msg))
            {
                return false;
            }
            if (this.number != null ? !this.number.Equals(target.number) : target.number != null)
            {
                return false;
            }
            if (this.origin != null ? !this.origin.Equals(target.origin) : target.origin != null)
            {
                return false;
            }
            if (this.prev_hash != null ? !this.prev_hash.Equals(target.prev_hash) : target.prev_hash != null)
            {
                return false;
            }
            if (this.deposit != null ? !this.deposit.Equals(target.deposit) : target.deposit != null)
            {
                return false;
            }

            return this.timestamp != null ? this.timestamp.Equals(target.timestamp) : target.timestamp == null;
        }

        public override string ToString()
        {
            return "ProgramInvokeImpl{" +
                    "address=" + this.address +
                    ", origin=" + this.origin +
                    ", caller=" + this.caller +
                    ", balance=" + this.balance +
                    ", callValue=" + this.call_value +
                    ", msgData=" + this.msg.ToString() +
                    ", prevHash=" + this.prev_hash +
                    ", coinbase=" + this.coinbase +
                    ", timestamp=" + this.timestamp +
                    ", number=" + this.number +
                    ", byTransaction=" + this.by_transaction +
                    ", byTestingSuite=" + this.is_testing_suite +
                    ", callDeep=" + this.call_deep +
                    '}';
        }
        #endregion
    }
}
