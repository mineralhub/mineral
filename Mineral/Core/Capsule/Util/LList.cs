using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Capsule.Util
{
    public class LList
    {
        #region Field
        private readonly byte[] rlp = null;
        private readonly int[] offsets = new int[32];
        private readonly int[] lens = new int[32];
        private int count = 0;
        #endregion


        #region Property
        public int Count => this.count;
        #endregion


        #region Contructor
        public LList(byte[] rlp)
        {
            this.rlp = rlp;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Add(int offset, int length, bool is_list)
        {
            this.offsets[this.count] = offset;
            this.lens[this.count] = is_list ? (-1 - length) : length;
            this.count++;
        }

        public byte[] GetEncoded()
        {
            byte[][] encoded = new byte[this.count][];
            for (int i = 0; i < this.count; i++)
            {
                encoded[i] = RLP.EncodeElement(GetBytes(i));
            }

            return RLP.EncodeList(encoded);
        }

        public byte[] GetBytes(int index)
        {
            int length = this.lens[index];
            length = length < 0 ? (-length - 1) : length;

            byte[] result = new byte[length];
            Array.Copy(this.rlp, this.offsets[index], result, 0, length);

            return result;
        }

        public LList GetList(int index)
        {
            RLPCollection collection = new RLPCollection();
            RLP.Decode(this.rlp, 0, this.offsets[index], -this.lens[index] - 1, 0, collection);

            return new LList(collection.RLPData);
        }

        public bool IsList(int index)
        {
            return this.lens[index] < 0;
        }
        #endregion
    }
}
