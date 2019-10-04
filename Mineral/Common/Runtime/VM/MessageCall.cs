using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM
{
    public class MessageCall
    {
        #region Field
        private readonly OpCode type;
        private readonly DataWord energy;
        private readonly DataWord code_address;
        private readonly DataWord endowment;
        private readonly DataWord in_data_offset;
        private readonly DataWord in_data_size;
        private DataWord out_data_offset;
        private DataWord out_data_size;
        private DataWord token_id;
        private bool is_token_transfer;
        #endregion


        #region Property
        public OpCode Type => this.type;
        public DataWord Energy => this.energy;
        public DataWord CodeAddress => this.code_address;
        public DataWord Endowment => this.endowment;
        public DataWord InDataOffset => this.in_data_offset;
        public DataWord InDataSize => this.in_data_size;
        public DataWord OutDataOffset => this.out_data_offset;
        public DataWord OutDataSize => this.out_data_size;
        public DataWord TokenId => this.token_id;
        public bool IsTokenTransfer => this.is_token_transfer;
        #endregion


        #region Contructor
        public MessageCall(OpCode type,
                           DataWord energy,
                           DataWord code_address,
                           DataWord endowment,
                           DataWord in_data_offset,
                           DataWord in_data_size,
                           DataWord token_id,
                           bool is_token_transfer)
        {
            this.type = type;
            this.energy = energy;
            this.code_address = code_address;
            this.endowment = endowment;
            this.in_data_offset = in_data_offset;
            this.in_data_size = in_data_size;
            this.token_id = token_id;
            this.is_token_transfer = is_token_transfer;
        }

        public MessageCall(OpCode type,
                           DataWord energy,
                           DataWord code_address,
                           DataWord endowment,
                           DataWord in_data_offset,
                           DataWord in_data_size,
                           DataWord out_data_offset,
                           DataWord out_data_size,
                           DataWord tokenId,
                           bool is_token_transfer)
            : this(type, energy, code_address, endowment, in_data_offset, in_data_size, tokenId, is_token_transfer)
        {
            this.out_data_offset = out_data_offset;
            this.out_data_size = out_data_size;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
