using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Utils;

namespace Mineral.Common.Runtime.VM.Program.Listener
{
    public class ProgramStorageChangeListener : ProgramListenerAdaptor
    {
        #region Field
        private Dictionary<DataWord, DataWord> difference = new Dictionary<DataWord, DataWord>();
        #endregion


        #region Property
        public Dictionary<DataWord, DataWord> Defference => difference;
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override void OnStoragePut(DataWord key, DataWord value)
        {
            this.difference.Put(key, value);
        }

        public void Merge(Dictionary<DataWord, DataWord> other)
        {
            this.difference.PutAll(other);
        }
        #endregion
    }
}
