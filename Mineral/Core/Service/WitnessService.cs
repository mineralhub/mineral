using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Application;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;

namespace Mineral.Core.Service
{
    public class WitnessService : IService
    {
        #region Field
        private IApplication application;
        private Manager db_manager;
        #endregion


        #region Property
        #endregion


        #region Constructor
        public WitnessService(Application application)
        {
            this.application = application;
            this.db_manager = application.Manager;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Init()
        {
            throw new NotImplementedException();
        }

        public void Init(Args args)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
