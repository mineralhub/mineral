using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Utils;

namespace Mineral.Core.Capsule
{
    public class CodeCapsule : IProtoCapsule<byte[]>
    {
        #region Field
        private byte[] code = null;
        #endregion


        #region Property
        public byte[] Instance => this.code;
        public byte[] Data => this.code;
        #endregion


        #region Contructor
        public CodeCapsule(byte[] code)
        {
            this.code = code;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public SHA256Hash GetCodeHash()
        {
            return SHA256Hash.Of(this.code);
        }

        public override string ToString()
        {
            return this.code.ToString();
        }
        #endregion
    }
}
