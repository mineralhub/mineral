using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Config.Arguments;

namespace Mineral.Common.Application
{
    public class ServiceContainer
    {
        #region Field
        private List<IService> services = new List<IService>();
        #endregion


        #region Property
        #endregion


        #region Constructor
        public ServiceContainer() { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Add(IService service)
        {
            this.services.Add(service);
        }

        public void Init()
        {
            foreach (IService service in this.services)
            {
                Logger.Debug("Init " + service.GetType().Name);
                service.Init();
            }
        }

        public void Init(Args args)
        {
            foreach (IService service in this.services)
            {
                Logger.Debug("Init " + service.GetType().Name);
                service.Init(args);
            }
        }

        public void Start()
        {
            Logger.Debug("Starting services");
            foreach (IService service in this.services)
            {
                Logger.Debug("Starting " + service.GetType().Name);
                service.Start();
            }
        }

        public void Stop()
        {
            foreach (IService service in this.services)
            {
                Logger.Debug("Stopping " + service.GetType().Name);
                service.Stop();
            }
        }
        #endregion
    }
}
