using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Exception;

namespace Mineral.Core.Database2.Core
{
    public class Session : ISession
    {
        #region Field
        private SnapshotManager snapshot_manager = null;
        private bool apply_snapshot = true;
        private bool disable_exit = false;
        #endregion


        #region Property
        #endregion


        #region Constructor
        public Session(SnapshotManager snapshot_manager)
            : this (snapshot_manager, false)
        {
        }

        public Session(SnapshotManager snapshot_manager, bool disable_exit)
        {
            this.snapshot_manager = snapshot_manager;
            this.disable_exit = disable_exit;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Close()
        {
            try
            {
                if (this.apply_snapshot)
                    this.snapshot_manager.Revoke();
            }
            catch (System.Exception e)
            {
                Logger.Error(string.Format("Revoke database error : {0}", e.Message));
                throw new RevokingStoreIllegalStateException(e.Message);
            }

            if (this.disable_exit)
                this.snapshot_manager.Disable();
        }

        public void Commit()
        {
            this.apply_snapshot = false;
            this.snapshot_manager.Commit();
        }

        public void Destroy()
        {
            try
            {
                if (this.apply_snapshot)
                    this.snapshot_manager.Revoke();
            }
            catch (System.Exception e)
            {
                Logger.Error(string.Format("Revoke database error : {0}", e.Message));
            }

            if (this.disable_exit)
                this.snapshot_manager.Disable();
        }

        public void Dispose()
        {
            Close();
        }

        public void Merge()
        {
            if (this.apply_snapshot)
                this.snapshot_manager.Merge();

            this.apply_snapshot = false;
        }

        public void Revoke()
        {
            if (this.apply_snapshot)
                this.snapshot_manager.Revoke();

            this.apply_snapshot = false;
        }
        #endregion
    }
}
