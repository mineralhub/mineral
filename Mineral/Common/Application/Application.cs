using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Mineral.Core;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;

namespace Mineral.Common.Application
{
    public class Application : IApplication
    {
        #region Field
        private DatabaseManager db_manager = Manager.Instance.DBManager;
        private BlockStore block_store = null;
        private ServiceContainer services = null;
        #endregion


        #region Property
        public DatabaseManager DBManager { get { return this.db_manager; } }
        public BlockStore BlockStore { get { return this.block_store; } }
        #endregion

        #region Constructor
        public Application()
        {
            this.block_store = this.db_manager.Block;
            this.services = new ServiceContainer();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void SetOption(Args args)
        {
            throw new NotImplementedException();
        }

        public void Startup()
        {
            Manager.Instance.NetService.Start();
        }

        public void Shutdown()
        {
            Logger.Info("------------------ Begin to shutdown ------------------");

            Manager.Instance.NetService.Close();
            //lock(this.db_manager.RevokeStore)
            //{
            //    this.db_manager.RevokeStore.Shutdown();
            //    this.db_manager.CloseAll();
            //}
            
            //this.db_manager.stopRepushThread();
            //this.db_manager.stopRepushTriggerThread();
            //EventPluginLoader.getInstance().stopPlugin();

            Logger.Info("------------------ End to shutdown------------------");
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
