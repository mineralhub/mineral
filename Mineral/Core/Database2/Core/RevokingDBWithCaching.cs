using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database2.Common;

namespace Mineral.Core.Database2.Core
{
    public class RevokingDBWithCaching
    {
        #region Field
        private ISnapshot head = null;
        private ThreadLocal<bool> mode = new ThreadLocal<bool>();
        #endregion


        #region Property
        public string DBName { get; set; }
        #endregion


        #region Constructor
        public RevokingDBWithCaching(string db_name, Type db_type)
        {
            DBName = db_name;
            //this.head = new SnapshotRoot(Args.Instance.)
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
