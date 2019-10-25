using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Program
{
    public class ProgramPrecompile
    {
        #region Field
        private HashSet<int> jumpdest = new HashSet<int>();
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public bool HasJumpDest(int pc)
        {
            return this.jumpdest.Contains(pc);
        }

        public static ProgramPrecompile Compile(byte[] ops)
        {
            ProgramPrecompile result = new ProgramPrecompile();
            for (int i = 0; i < ops.Length; ++i)
            {
                if (!Enum.IsDefined(typeof(OpCode), ops[i]))
                    continue;

                OpCode op = (OpCode)ops[i];
                if (op.Equals(OpCode.JUMPDEST))
                {
                    Logger.Debug("JUMPDEST:" + i);
                    result.jumpdest.Add(i);
                }

                if (op >= OpCode.PUSH1 && op <= OpCode.PUSH32)
                {
                    i += (int)op - (int)OpCode.PUSH1 + 1;
                }
            }
            return result;
        }

        public static byte[] GetCode(byte[] ops)
        {
            for (int i = 0; i < ops.Length; ++i)
            {
                if (!Enum.IsDefined(typeof(OpCode), ops[i]))
                    continue;


                OpCode op = (OpCode)ops[i];
                if (op.Equals(OpCode.RETURN))
                {
                    Logger.Debug("Op code : return");
                }

                if (op.Equals(OpCode.RETURN)
                    && i + 1 < ops.Length
                    && Enum.IsDefined(typeof(OpCode), ops[i + 1])
                    && ((OpCode)ops[i + 1]).Equals(OpCode.STOP))
                {
                    byte[] result = null;
                    i++;
                    result = new byte[ops.Length - i - 1];

                    Array.Copy(ops, i + 1, result, 0, ops.Length - i - 1);
                    return result;
                }

                if (op >= OpCode.PUSH1 && op <= OpCode.PUSH32)
                {
                    i += (int)op - (int)OpCode.PUSH1 + 1;
                }
            }
            return new DataWord(0).Data;
        }
        #endregion
    }
}
