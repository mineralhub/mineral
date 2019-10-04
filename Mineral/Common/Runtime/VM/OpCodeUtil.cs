using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static Mineral.Common.Runtime.VM.OpCodeAttribute;

namespace Mineral.Common.Runtime.VM
{
    public static class OpCodeUtil
    {
        public static OpCodeAttribute GetOpCodeAttribute(OpCode code)
        {
            FieldInfo info = typeof(OpCode).GetField(code.ToString());
            if (info == null)
                throw new System.Exception("Invalid OpCode");

            OpCodeAttribute attribute = (OpCodeAttribute)info.GetCustomAttribute(typeof(OpCodeAttribute));
            if (attribute == null)
                throw new System.Exception("Invalid OpCodeAttribute type");

            return attribute;
        }

        public static byte ToCode(OpCode code)
        {
            OpCodeAttribute attribute =  GetOpCodeAttribute(code);

            return attribute.OpCode;
        }

        public static int ToRequire(OpCode code)
        {
            OpCodeAttribute attribute = GetOpCodeAttribute(code);

            return attribute.Require;
        }

        public static int ToResult(OpCode code)
        {
            OpCodeAttribute attribute = GetOpCodeAttribute(code);

            return attribute.Result;
        }

        public static OpCodeAttribute.Tier ToTier(OpCode code)
        {
            OpCodeAttribute attribute = GetOpCodeAttribute(code);

            return attribute.OpCodeTier;
        }

        private static bool IsCall(OpCodeAttribute attribute)
        {
            return attribute.Flags.Contains(CallFlags.Call);
        }

        private static void CheckCall(OpCodeAttribute attribute)
        {
            if (!IsCall(attribute))
            {
                throw new System.Exception("OpCode is not a call : " + attribute.OpCode.ToString());
            }
        }

        public static bool ContainStateless(OpCode code)
        {
            OpCodeAttribute attribute = GetOpCodeAttribute(code);
            CheckCall(attribute);

            return attribute.Flags.Contains(CallFlags.Stateless);
        }

        public static bool ContainHasValue(OpCode code)
        {
            OpCodeAttribute attribute = GetOpCodeAttribute(code);
            CheckCall(attribute);

            return attribute.Flags.Contains(CallFlags.HasValue);
        }

        public static bool ContainStatic(OpCode code)
        {
            OpCodeAttribute attribute = GetOpCodeAttribute(code);
            CheckCall(attribute);

            return attribute.Flags.Contains(CallFlags.Static);
        }

        public static bool ContainDelegate(OpCode code)
        {
            OpCodeAttribute attribute = GetOpCodeAttribute(code);
            CheckCall(attribute);

            return attribute.Flags.Contains(CallFlags.Delegate);
        }
    }
}
