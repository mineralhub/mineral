using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Database
{
    public class Manager
    {
        #region Field
        public BlockStore block_store;
        private DynamicPropertiesStore dynamic_properties_store = new DynamicPropertiesStore("properties");
        #endregion


        #region Property
        public BlockStore BlockStore { get { return this.block_store; } }
        public DynamicPropertiesStore DynamicPropertiesStore { get { return this.dynamic_properties_store; } }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
