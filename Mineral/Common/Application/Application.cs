using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;

namespace Mineral.Common.Application
{
    public class Application : IApplication
    {
        #region Field
        private DataBaseManager db_manager;
        private BlockStore block_store = null;
        private ServiceContainer services = null;
        private bool is_producer = false;
        #endregion


        #region Property
        public DataBaseManager Manager { get { return this.db_manager; } }
        public BlockStore BlockStore { get { return this.block_store; } }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Init(Args args)
        {
            this.block_store = this.Manager.BlockStore;
            this.services = new ServiceContainer();
        }

        public void SetOption(Args args)
        {
            throw new NotImplementedException();
        }

        public void Startup()
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void InitService(Args args)
        {
            this.services.Init(args);
        }

        public void AddService(IService service)
        {
            this.services.Add(service);
        }

        public void StartService()
        {
            this.services.Start();
        }

        public void ShutdownService()
        {
            this.services.Stop();
        }
        #endregion
    }
}
