using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Database2.Core
{
    public interface ISnapshot
    {
        byte[] Get(byte[] key);
        void Put(byte[] key, byte[] value);

        void Remove(byte[] key);
        void Merge(ISnapshot snapshot);
        void SetPrevious(ISnapshot snapshot);
        void SetNext(ISnapshot next);
        void Close();
        void Reset();
        void ResetSolidity();
        void UpdateSolidity();

        ISnapshot Advance();
        ISnapshot Retreat();
        ISnapshot GetPrevious();
        ISnapshot GetNext();
        ISnapshot GetSolidity();

    }
}
