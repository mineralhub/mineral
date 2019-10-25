using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM
{
    //Instruction set for the Ethereum Virtual Machine See Yellow Paper:
    //http://www.gavwood.com/Paper.pdf - Appendix G. Virtual Machine Specification
    public enum OpCode
    {
        // TODO #POC9 Need to make tiers more accurate
        /**
         * Halts execution (0x00)
         */
        [OpCodeAttribute(0x00, 0, 0, OpCodeAttribute.Tier.ZeroTier)]
        STOP = 0x00,

        /*  Arithmetic Operations   */

        /**
         * (0x01) Addition operation
         */
        [OpCodeAttribute(0x01, 2, 1, OpCodeAttribute.Tier.VeryLowTier)]
        ADD = 0x01,
        /**
         * (0x02) Multiplication operation
         */
        [OpCodeAttribute(0x02, 2, 1, OpCodeAttribute.Tier.LowTier)]
        MUL = 0x02,
        /**
         * (0x03) Subtraction operations
         */
        [OpCodeAttribute(0x03, 2, 1, OpCodeAttribute.Tier.VeryLowTier)]
        SUB = 0x03,
        /**
         * (0x04) Integer division operation
         */
        [OpCodeAttribute(0x04, 2, 1, OpCodeAttribute.Tier.LowTier)]
        DIV = 0x04,
        /**
         * (0x05) Signed integer division operation
         */
        [OpCodeAttribute(0x05, 2, 1, OpCodeAttribute.Tier.LowTier)]
        SDIV = 0x05,
        /**
         * (0x06) Modulo remainder operation
         */
        [OpCodeAttribute(0x06, 2, 1, OpCodeAttribute.Tier.LowTier)]
        MOD = 0x06,
        /**
         * (0x07) Signed modulo remainder operation
         */
        [OpCodeAttribute(0x07, 2, 1, OpCodeAttribute.Tier.LowTier)]
        SMOD = 0x07,
        /**
         * (0x08) Addition combined with modulo remainder operation
         */
        [OpCodeAttribute(0x08, 3, 1, OpCodeAttribute.Tier.MidTier)]
        ADDMOD = 0x08,
        /**
         * (0x09) Multiplication combined with modulo remainder operation
         */
        [OpCodeAttribute(0x09, 3, 1, OpCodeAttribute.Tier.MidTier)]
        MULMOD = 0x09,
        /**
         * (0x0a) Exponential operation
         */
        [OpCodeAttribute(0x0A, 3, 1, OpCodeAttribute.Tier.SpecialTier)]
        EXP = 0x0A,
        /**
         * (0x0b) Extend length of signed integer
         */
        [OpCodeAttribute(0x0B, 3, 1, OpCodeAttribute.Tier.LowTier)]
        SIGNEXTEND = 0x0B,

        /*  Bitwise Logic & Comparison Operations   */

        /**
         * (0x10) Less-than comparison
         */
        [OpCodeAttribute(0x10, 2, 1, OpCodeAttribute.Tier.VeryLowTier)]
        LT = 0x10,
        /**
         * (0x11) Greater-than comparison
         */
        [OpCodeAttribute(0x11, 2, 1, OpCodeAttribute.Tier.VeryLowTier)]
        GT = 0x11,
        /**
         * (0x12) Signed less-than comparison
         */
        [OpCodeAttribute(0x12, 2, 1, OpCodeAttribute.Tier.VeryLowTier)]
        SLT = 0x12,
        /**
         * (0x13) Signed greater-than comparison
         */
        [OpCodeAttribute(0x13, 2, 1, OpCodeAttribute.Tier.VeryLowTier)]
        SGT = 0x13,
        /**
         * (0x14) Equality comparison
         */
        [OpCodeAttribute(0x14, 2, 1, OpCodeAttribute.Tier.VeryLowTier)]
        EQ = 0x14,
        /**
         * (0x15) Negation operation
         */
        [OpCodeAttribute(0x15, 1, 1, OpCodeAttribute.Tier.VeryLowTier)]
        ISZERO = 0x15,
        /**
         * (0x16) Bitwise AND operation
         */
        [OpCodeAttribute(0x16, 2, 1, OpCodeAttribute.Tier.VeryLowTier)]
        AND = 0x16,
        /**
         * (0x17) Bitwise OR operation
         */
        [OpCodeAttribute(0x17, 2, 1, OpCodeAttribute.Tier.VeryLowTier)]
        OR = 0x17,
        /**
         * (0x18) Bitwise XOR operation
         */
        [OpCodeAttribute(0x18, 2, 1, OpCodeAttribute.Tier.VeryLowTier)]
        XOR = 0x18,
        /**
         * (0x19) Bitwise NOT operationr
         */
        [OpCodeAttribute(0x19, 1, 1, OpCodeAttribute.Tier.VeryLowTier)]
        NOT = 0x19,
        /**
         * (0x1a) Retrieve single byte from word
         */
        [OpCodeAttribute(0x1A, 2, 1, OpCodeAttribute.Tier.VeryLowTier)]
        BYTE = 0x1A,
        /**
         * (0x1b) Shift left
         */
        [OpCodeAttribute(0x1B, 2, 1, OpCodeAttribute.Tier.VeryLowTier)]
        SHL = 0x1B,
        /**
         * (0x1c) Logical shift right
         */
        [OpCodeAttribute(0x1C, 2, 1, OpCodeAttribute.Tier.VeryLowTier)]
        SHR = 0x1C,
        /**
         * (0x1d) Arithmetic shift right
         */
        [OpCodeAttribute(0x1D, 2, 1, OpCodeAttribute.Tier.VeryLowTier)]
        SAR = 0x1D,

        /*  Cryptographic Operations    */

        /**
         * (0x20) Compute SHA3-256 hash
         */
        [OpCodeAttribute(0x20, 2, 1, OpCodeAttribute.Tier.SpecialTier)]
        SHA3 = 0x20,

        /*  Environmental Information   */

        /**
         * (0x30)  Get address of currently executing account
         */
        [OpCodeAttribute(0x30, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        ADDRESS = 0x30,
        /**
         * (0x31) Get balance of the given account
         */
        [OpCodeAttribute(0x31, 1, 1, OpCodeAttribute.Tier.ExtTier)]
        BALANCE = 0x31,
        /**
         * (0x32) Get execution origination address
         */
        [OpCodeAttribute(0x32, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        ORIGIN = 0x32,
        /**
         * (0x33) Get caller address
         */
        [OpCodeAttribute(0x33, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        CALLER = 0x33,
        /**
         * (0x34) Get deposited value by the instruction/transaction responsible for this execution
         */
        [OpCodeAttribute(0x34, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        CALLVALUE = 0x34,
        /**
         * (0x35) Get input data of current environment
         */
        [OpCodeAttribute(0x35, 1, 1, OpCodeAttribute.Tier.VeryLowTier)]
        CALLDATALOAD = 0x35,
        /**
         * (0x36) Get size of input data in current environment
         */
        [OpCodeAttribute(0x36, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        CALLDATASIZE = 0x36,
        /**
         * (0x37) Copy input data in current environment to memory
         */
        [OpCodeAttribute(0x37, 3, 0, OpCodeAttribute.Tier.VeryLowTier)]
        CALLDATACOPY = 0x37,
        /**
         * (0x38) Get size of code running in current environment
         */
        [OpCodeAttribute(0x38, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        CODESIZE = 0x38,
        /**
         * (0x39) Copy code running in current environment to memory
         */
        [OpCodeAttribute(0x39, 3, 0, OpCodeAttribute.Tier.VeryLowTier)]
        CODECOPY = 0x39, // [len code_start mem_start CODECOPY]

        [OpCodeAttribute(0x3D, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        RETURNDATASIZE = 0x3D,

        [OpCodeAttribute(0x3E, 3, 0, OpCodeAttribute.Tier.VeryLowTier)]
        RETURNDATACOPY = 0x3E,
        /**
         * (0x3a) Get price of gas in current environment
         */
        [OpCodeAttribute(0x3A, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        GASPRICE = 0x3A,
        /**
         * (0x3b) Get size of code running in current environment with given offset
         */
        [OpCodeAttribute(0x3B, 1, 1, OpCodeAttribute.Tier.ExtTier)]
        EXTCODESIZE = 0x3B,
        /**
         * (0x3c) Copy code running in current environment to memory with given offset
         */
        [OpCodeAttribute(0x3C, 4, 0, OpCodeAttribute.Tier.ExtTier)]
        EXTCODECOPY = 0x3C,
        /**
         * (0x3f) Returns the keccak256 hash of a contract’s code
         */
        [OpCodeAttribute(0x3F, 1, 1, OpCodeAttribute.Tier.ExtTier)]
        EXTCODEHASH = 0x3F,

        /*  Block Information   */

        /**
         * (0x40) Get hash of most recent complete block
         */
        [OpCodeAttribute(0x40, 1, 1, OpCodeAttribute.Tier.ExtTier)]
        BLOCKHASH = 0x40,
        /**
         * (0x41) Get the block’s coinbase address
         */
        [OpCodeAttribute(0x41, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        COINBASE = 0x41,
        /**
         * (x042) Get the block’s timestamp
         */
        [OpCodeAttribute(0x42, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        TIMESTAMP = 0x42,
        /**
         * (0x43) Get the block’s number
         */
        [OpCodeAttribute(0x43, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        NUMBER = 0x43,
        /**
         * (0x44) Get the block’s difficulty
         */
        [OpCodeAttribute(0x44, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        DIFFICULTY = 0x44,
        /**
         * (0x45) Get the block’s gas limit
         */
        [OpCodeAttribute(0x45, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        GASLIMIT = 0x45,

        /*  Memory, Storage and Flow Operations */

        /**
         * (0x50) Remove item from stack
         */
        [OpCodeAttribute(0x50, 1, 0, OpCodeAttribute.Tier.BaseTier)]
        POP = 0x50,
        /**
         * (0x51) Load word from memory
         */
        [OpCodeAttribute(0x51, 1, 1, OpCodeAttribute.Tier.VeryLowTier)]
        MLOAD = 0x51,
        /**
         * (0x52) Save word to memory
         */
        [OpCodeAttribute(0x52, 2, 0, OpCodeAttribute.Tier.VeryLowTier)]
        MSTORE = 0x52,
        /**
         * (0x53) Save byte to memory
         */
        [OpCodeAttribute(0x53, 2, 0, OpCodeAttribute.Tier.VeryLowTier)]
        MSTORE8 = 0x53,
        /**
         * (0x54) Load word from storage
         */
        [OpCodeAttribute(0x54, 1, 1, OpCodeAttribute.Tier.SpecialTier)]
        SLOAD = 0x54,
        /**
         * (0x55) Save word to storage
         */
        [OpCodeAttribute(0x55, 2, 0, OpCodeAttribute.Tier.SpecialTier)]
        SSTORE = 0x55,
        /**
         * (0x56) Alter the program counter
         */
        [OpCodeAttribute(0x56, 1, 0, OpCodeAttribute.Tier.MidTier)]
        JUMP = 0x56,
        /**
         * (0x57) Conditionally alter the program counter
         */
        [OpCodeAttribute(0x57, 2, 0, OpCodeAttribute.Tier.HighTier)]
        JUMPI = 0x57,
        /**
         * (0x58) Get the program counter
         */
        [OpCodeAttribute(0x58, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        PC = 0x58,
        /**
         * (0x59) Get the size of active memory
         */
        [OpCodeAttribute(0x59, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        MSIZE = 0x59,
        /**
         * (0x5a) Get the amount of available gas
         */
        [OpCodeAttribute(0x5A, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        GAS = 0x5A,
        /**
         * (0x5b)
         */
        [OpCodeAttribute(0x5B, 0, 0, OpCodeAttribute.Tier.SpecialTier)]
        JUMPDEST = 0x5B,

        /*  Push Operations */

        /**
         * (0x60) Place 1-byte item on stack
         */
        [OpCodeAttribute(0x60, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH1 = 0x60,
        /**
         * (0x61) Place 2-byte item on stack
         */
        [OpCodeAttribute(0x61, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH2 = 0x61,
        /**
         * (0x62) Place 3-byte item on stack
         */
        [OpCodeAttribute(0x62, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH3 = 0x62,
        /**
         * (0x63) Place 4-byte item on stack
         */
        [OpCodeAttribute(0x63, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH4 = 0x63,
        /**
         * (0x64) Place 5-byte item on stack
         */
        [OpCodeAttribute(0x64, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH5 = 0x64,
        /**
         * (0x65) Place 6-byte item on stack
         */
        [OpCodeAttribute(0x65, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH6 = 0x65,
        /**
         * (0x66) Place 7-byte item on stack
         */
        [OpCodeAttribute(0x66, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH7 = 0x66,
        /**
         * (0x67) Place 8-byte item on stack
         */
        [OpCodeAttribute(0x67, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH8 = 0x67,
        /**
         * (0x68) Place 9-byte item on stack
         */
        [OpCodeAttribute(0x68, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH9 = 0x68,
        /**
         * (0x69) Place 10-byte item on stack
         */
        [OpCodeAttribute(0x69, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH10 = 0x69,
        /**
         * (0x6a) Place 11-byte item on stack
         */
        [OpCodeAttribute(0x6A, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH11 = 0x6A,
        /**
         * (0x6b) Place 12-byte item on stack
         */
        [OpCodeAttribute(0x6B, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH12 = 0x6B,
        /**
         * (0x6c) Place 13-byte item on stack
         */
        [OpCodeAttribute(0x6C, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH13 = 0x6C,
        /**
         * (0x6d) Place 14-byte item on stack
         */
        [OpCodeAttribute(0x6D, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH14 = 0x6D,
        /**
         * (0x6e) Place 15-byte item on stack
         */
        [OpCodeAttribute(0x6E, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH15 = 0x6E,
        /**
         * (0x6f) Place 16-byte item on stack
         */
        [OpCodeAttribute(0x6F, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH16 = 0x6F,
        /**
         * (0x70) Place 17-byte item on stack
         */
        [OpCodeAttribute(0x70, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH17 = 0x70,
        /**
         * (0x71) Place 18-byte item on stack
         */
        [OpCodeAttribute(0x71, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH18 = 0x71,
        /**
         * (0x72) Place 19-byte item on stack
         */
        [OpCodeAttribute(0x72, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH19 = 0x72,
        /**
         * (0x73) Place 20-byte item on stack
         */
        [OpCodeAttribute(0x73, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH20 = 0x73,
        /**
         * (0x74) Place 21-byte item on stack
         */
        [OpCodeAttribute(0x74, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH21 = 0x74,
        /**
         * (0x75) Place 22-byte item on stack
         */
        [OpCodeAttribute(0x75, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH22 = 0x75,
        /**
         * (0x76) Place 23-byte item on stack
         */
        [OpCodeAttribute(0x76, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH23 = 0x76,
        /**
         * (0x77) Place 24-byte item on stack
         */
        [OpCodeAttribute(0x77, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH24 = 0x77,
        /**
         * (0x78) Place 25-byte item on stack
         */
        [OpCodeAttribute(0x78, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH25 = 0x78,
        /**
         * (0x79) Place 26-byte item on stack
         */
        [OpCodeAttribute(0x79, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH26 = 0x79,
        /**
         * (0x7a) Place 27-byte item on stack
         */
        [OpCodeAttribute(0x7A, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH27 = 0x7A,
        /**
         * (0x7b) Place 28-byte item on stack
         */
        [OpCodeAttribute(0x7B, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH28 = 0x7B,
        /**
         * (0x7c) Place 29-byte item on stack
         */
        [OpCodeAttribute(0x7C, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH29 = 0x7C,
        /**
         * (0x7d) Place 30-byte item on stack
         */
        [OpCodeAttribute(0x7D, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH30 = 0x7D,
        /**
         * (0x7e) Place 31-byte item on stack
         */
        [OpCodeAttribute(0x7E, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH31 = 0x7E,
        /**
         * (0x7f) Place 32-byte (full word) item on stack
         */
        [OpCodeAttribute(0x7F, 0, 1, OpCodeAttribute.Tier.VeryLowTier)]
        PUSH32 = 0x7F,

        /*  Duplicate Nth item from the stack   */

        /**
         * (0x80) Duplicate 1st item on stack
         */
        [OpCodeAttribute(0x80, 1, 2, OpCodeAttribute.Tier.VeryLowTier)]
        DUP1 = 0x80,
        /**
         * (0x81) Duplicate 2nd item on stack
         */
        [OpCodeAttribute(0x81, 2, 3, OpCodeAttribute.Tier.VeryLowTier)]
        DUP2 = 0x81,
        /**
         * (0x82) Duplicate 3rd item on stack
         */
        [OpCodeAttribute(0x82, 3, 4, OpCodeAttribute.Tier.VeryLowTier)]
        DUP3 = 0x82,
        /**
         * (0x83) Duplicate 4th item on stack
         */
        [OpCodeAttribute(0x83, 4, 5, OpCodeAttribute.Tier.VeryLowTier)]
        DUP4 = 0x83,
        /**
         * (0x84) Duplicate 5th item on stack
         */
        [OpCodeAttribute(0x84, 5, 6, OpCodeAttribute.Tier.VeryLowTier)]
        DUP5 = 0x84,
        /**
         * (0x85) Duplicate 6th item on stack
         */
        [OpCodeAttribute(0x85, 6, 7, OpCodeAttribute.Tier.VeryLowTier)]
        DUP6 = 0x85,
        /**
         * (0x86) Duplicate 7th item on stack
         */
        [OpCodeAttribute(0x86, 7, 8, OpCodeAttribute.Tier.VeryLowTier)]
        DUP7 = 0x86,
        /**
         * (0x87) Duplicate 8th item on stack
         */
        [OpCodeAttribute(0x87, 8, 9, OpCodeAttribute.Tier.VeryLowTier)]
        DUP8 = 0x87,
        /**
         * (0x88) Duplicate 9th item on stack
         */
        [OpCodeAttribute(0x88, 9, 10, OpCodeAttribute.Tier.VeryLowTier)]
        DUP9 = 0x88,
        /**
         * (0x89) Duplicate 10th item on stack
         */
        [OpCodeAttribute(0x89, 10, 11, OpCodeAttribute.Tier.VeryLowTier)]
        DUP10 = 0x89,
        /**
         * (0x8a) Duplicate 11th item on stack
         */
        [OpCodeAttribute(0x8A, 11, 12, OpCodeAttribute.Tier.VeryLowTier)]
        DUP11 = 0x8A,
        /**
         * (0x8b) Duplicate 12th item on stack
         */
        [OpCodeAttribute(0x8B, 12, 13, OpCodeAttribute.Tier.VeryLowTier)]
        DUP12 = 0x8B,
        /**
         * (0x8c) Duplicate 13th item on stack
         */
        [OpCodeAttribute(0x8C, 13, 14, OpCodeAttribute.Tier.VeryLowTier)]
        DUP13 = 0x8C,
        /**
         * (0x8d) Duplicate 14th item on stack
         */
        [OpCodeAttribute(0x8D, 14, 15, OpCodeAttribute.Tier.VeryLowTier)]
        DUP14 = 0x8D,
        /**
         * (0x8e) Duplicate 15th item on stack
         */
        [OpCodeAttribute(0x8E, 15, 16, OpCodeAttribute.Tier.VeryLowTier)]
        DUP15 = 0x8E,
        /**
         * (0x8f) Duplicate 16th item on stack
         */
        [OpCodeAttribute(0x8F, 16, 17, OpCodeAttribute.Tier.VeryLowTier)]
        DUP16 = 0x8F,

        /*  Swap the Nth item from the stack with the top   */

        /**
         * (0x90) Exchange 2nd item from stack with the top
         */
        [OpCodeAttribute(0x90, 2, 2, OpCodeAttribute.Tier.VeryLowTier)]
        SWAP1 = 0x90,
        /**
         * (0x91) Exchange 3rd item from stack with the top
         */
        [OpCodeAttribute(0x91, 3, 3, OpCodeAttribute.Tier.VeryLowTier)]
        SWAP2 = 0x91,
        /**
         * (0x92) Exchange 4th item from stack with the top
         */
        [OpCodeAttribute(0x92, 4, 4, OpCodeAttribute.Tier.VeryLowTier)]
        SWAP3 = 0x92,
        /**
         * (0x93) Exchange 5th item from stack with the top
         */
        [OpCodeAttribute(0x93, 5, 5, OpCodeAttribute.Tier.VeryLowTier)]
        SWAP4 = 0x93,
        /**
         * (0x94) Exchange 6th item from stack with the top
         */
        [OpCodeAttribute(0x94, 6, 6, OpCodeAttribute.Tier.VeryLowTier)]
        SWAP5 = 0x94,
        /**
         * (0x95) Exchange 7th item from stack with the top
         */
        [OpCodeAttribute(0x95, 7, 7, OpCodeAttribute.Tier.VeryLowTier)]
        SWAP6 = 0x95,
        /**
         * (0x96) Exchange 8th item from stack with the top
         */
        [OpCodeAttribute(0x96, 8, 8, OpCodeAttribute.Tier.VeryLowTier)]
        SWAP7 = 0x96,
        /**
         * (0x97) Exchange 9th item from stack with the top
         */
        [OpCodeAttribute(0x97, 9, 9, OpCodeAttribute.Tier.VeryLowTier)]
        SWAP8 = 0x97,
        /**
         * (0x98) Exchange 10th item from stack with the top
         */
        [OpCodeAttribute(0x98, 10, 10, OpCodeAttribute.Tier.VeryLowTier)]
        SWAP9 = 0x98,
        /**
         * (0x99) Exchange 11th item from stack with the top
         */
        [OpCodeAttribute(0x99, 11, 11, OpCodeAttribute.Tier.VeryLowTier)]
        SWAP10 = 0x99,
        /**
         * (0x9a) Exchange 12th item from stack with the top
         */
        [OpCodeAttribute(0x9A, 12, 12, OpCodeAttribute.Tier.VeryLowTier)]
        SWAP11 = 0x9A,
        /**
         * (0x9b) Exchange 13th item from stack with the top
         */
        [OpCodeAttribute(0x9B, 13, 13, OpCodeAttribute.Tier.VeryLowTier)]
        SWAP12 = 0x9B,
        /**
         * (0x9c) Exchange 14th item from stack with the top
         */
        [OpCodeAttribute(0x9C, 14, 14, OpCodeAttribute.Tier.VeryLowTier)]
        SWAP13 = 0x9C,
        /**
         * (0x9d) Exchange 15th item from stack with the top
         */
        [OpCodeAttribute(0x9D, 15, 15, OpCodeAttribute.Tier.VeryLowTier)]
        SWAP14 = 0x9D,
        /**
         * (0x9e) Exchange 16th item from stack with the top
         */
        [OpCodeAttribute(0x9E, 16, 16, OpCodeAttribute.Tier.VeryLowTier)]
        SWAP15 = 0x9E,
        /**
         * (0x9f) Exchange 17th item from stack with the top
         */
        [OpCodeAttribute(0x9F, 17, 17, OpCodeAttribute.Tier.VeryLowTier)]
        SWAP16 = 0x9F,

        /**
         * (0xa[n]) log some data for some addres with 0..n tags [addr [tag0..tagn] data]
         */
        [OpCodeAttribute(0xA0, 2, 0, OpCodeAttribute.Tier.SpecialTier)]
        LOG0 = 0xA0,
        [OpCodeAttribute(0xA1, 3, 0, OpCodeAttribute.Tier.SpecialTier)]
        LOG1 = 0xA1,
        [OpCodeAttribute(0xA2, 4, 0, OpCodeAttribute.Tier.SpecialTier)]
        LOG2 = 0xA2,
        [OpCodeAttribute(0xA3, 5, 0, OpCodeAttribute.Tier.SpecialTier)]
        LOG3 = 0xA3,
        [OpCodeAttribute(0xA4, 6, 0, OpCodeAttribute.Tier.SpecialTier)]
        LOG4 = 0xA4,

        /*  System operations   */

        /**
         * (0xd0) Message-call into an account with trc10 token
         */
        [OpCodeAttribute(0xD0, 6, 0, OpCodeAttribute.Tier.SpecialTier, OpCodeAttribute.CallFlags.HasValue)]
        CALLTOKEN = 0xD0,
        [OpCodeAttribute(0xD1, 2, 1, OpCodeAttribute.Tier.ExtTier)]
        TOKENBALANCE = 0xD1,
        [OpCodeAttribute(0xD2, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        CALLTOKENVALUE = 0xD2,
        [OpCodeAttribute(0xD3, 0, 1, OpCodeAttribute.Tier.BaseTier)]
        CALLTOKENID = 0xD3,

        /**
         * (0xf0) Create a new account with associated code
         */
        [OpCodeAttribute(0xF0, 3, 1, OpCodeAttribute.Tier.SpecialTier)]
        CREATE = 0xF0,   //       [in_size] [in_offs] [gas_val] CREATE
                                                       /**
                                                        * (cxf1) Message-call into an account
                                                        */
        [OpCodeAttribute(0xF1, 7, 1, OpCodeAttribute.Tier.SpecialTier, OpCodeAttribute.CallFlags.Call, OpCodeAttribute.CallFlags.HasValue)]
        CALL = 0xF1,
        //       [out_data_size] [out_data_start] [in_data_size] [in_data_start] [value] [to_addr]
        // [gas] CALL
        /**
         * (0xf2) Calls self, but grabbing the code from the TO argument instead of from one's own
         * address
         */
        [OpCodeAttribute(0xF2, 7, 1, OpCodeAttribute.Tier.SpecialTier, OpCodeAttribute.CallFlags.Call, OpCodeAttribute.CallFlags.HasValue, OpCodeAttribute.CallFlags.Stateless)]
        CALLCODE = 0xF2,
        /**
         * (0xf3) Halt execution returning output data
         */
        [OpCodeAttribute(0xF3, 2, 0, OpCodeAttribute.Tier.ZeroTier)]
        RETURN = 0xF3,

        /**
         * (0xf4)  similar in idea to CALLCODE, except that it propagates the sender and value from the
         * parent scope to the child scope, ie. the call created has the same sender and value as the
         * original call. also the Value parameter is omitted for this opCode
         */
        [OpCodeAttribute(0xF4, 6, 1, OpCodeAttribute.Tier.SpecialTier, OpCodeAttribute.CallFlags.Call, OpCodeAttribute.CallFlags.Delegate, OpCodeAttribute.CallFlags.Stateless)]
        DELEGATECALL = 0xF4,

        /**
         * (0xf5) Skinny CREATE2, same as CREATE but with deterministic address
         */
        [OpCodeAttribute(0xF5, 4, 1, OpCodeAttribute.Tier.SpecialTier)]
        CREATE2 = 0xF5,

        /**
         * opcode that can be used to call another contract (or itself) while disallowing any
         * modifications to the state during the call (and its subcalls, if present). Any opcode that
         * attempts to perform such a modification (see below for details) will result in an exception
         * instead of performing the modification.
         */
        [OpCodeAttribute(0xFA, 6, 1, OpCodeAttribute.Tier.SpecialTier, OpCodeAttribute.CallFlags.Call, OpCodeAttribute.CallFlags.Static)]
        STATICCALL = 0xFA,

        /**
         * (0xfd) The `REVERT` instruction will stop execution, roll back all state changes done so far
         * and provide a pointer to a memory section, which can be interpreted as an error code or
         * message. While doing so, it will not consume all the remaining gas.
         */
        [OpCodeAttribute(0xFD, 2, 0, OpCodeAttribute.Tier.ZeroTier)]
        REVERT = 0xFD,
        /**
         * (0xff) Halt execution and register account for later deletion
         */
        [OpCodeAttribute(0xFF, 1, 0, OpCodeAttribute.Tier.ZeroTier)]
        SUICIDE = 0xFF,
    }
}

