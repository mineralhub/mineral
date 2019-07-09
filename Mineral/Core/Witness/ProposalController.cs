using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Capsule;
using Mineral.Core.Database;

namespace Mineral.Core.Witness
{
    public class ProposalController
    {
        #region Field
        private DataBaseManager db_manager = null;
        #endregion


        #region Property
        #endregion


        #region Contructor
        public ProposalController(DataBaseManager db_manager)
        {
            this.db_manager = db_manager;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void ProcessProposals()
        {
            long latest_num = this.db_manager.DynamicProperties.GetLatestProposalNum();
            if (latest_num == 0)
            {
                Logger.Info("latestProposalNum is 0,return");
                return;
            }

            long proposal_num = latest_num;

            ProposalCapsule proposal = null;

            while (proposal_num > 0)
            {
                try
                {
                    proposal = this.db_manager.Proposal.Get(ProposalCapsule.CalculateDatabaseKey(proposal_num));
                }
                catch (System.Exception e)
                {
                    Logger.Error(e.Message);
                    continue;
                }

                if (proposal.HasProcessed)
                {
                    Logger.Info(
                        string.Format("Proposal has processed，id:[{0}],skip it and before it",
                                      proposal.Id));
                    break;
                }

                if (proposal.HasCanceled)
                {
                    Logger.Info(
                        string.Format("Proposal has canceled，id:[{0}],skip it",
                                      proposal.Id));
                    proposal_num--;
                    continue;
                }

                long current_time = this.db_manager.DynamicProperties.GetNextMaintenanceTime();
                if (proposal.HasExpired(current_time))
                {
                    ProcessProposal(proposal);
                    proposal_num--;
                    continue;
                }

                proposal_num--;
                Logger.Info(
                    string.Format("Proposal has not expired，id:[{0}],skip it",
                                  proposal.Id));
            }

            Logger.Info(
                string.Format("Processing proposals done, oldest proposal[{0}]",
                              proposal_num));
        }

        public void ProcessProposal(ProposalCapsule proposal)
        {
            List<ByteString> active_witness = this.db_manager.WitnessSchedule.GetActiveWitnesses();
            if (proposal.HasMostApprovals(active_witness))
            {
                Logger.Info(
                    string.Format("Processing proposal,id:{0},it has received most approvals, "
                                  + "begin to set dynamic parameter:{1}, "
                                  + "and set proposal state as APPROVED",
                                  proposal.Id,
                                  proposal.Parameters));

                SetDynamicParameters(proposal);
                proposal.State = Protocol.Proposal.Types.State.Approved;
                this.db_manager.Proposal.Put(proposal.CreateDatabaseKey(), proposal);
            }
            else
            {
                Logger.Info(
                    string.Format("Processing proposal,id:{0}, "
                                  + "it has not received enough approvals, set proposal state as DISAPPROVED",
                                  proposal.Id));

                proposal.State = Protocol.Proposal.Types.State.Disapproved;
                this.db_manager.Proposal.Put(proposal.CreateDatabaseKey(), proposal);
            }
        }

        public void SetDynamicParameters(ProposalCapsule proposal)
        {
            foreach (KeyValuePair<long, long> pair in proposal.Instance.Parameters)
            {
                switch ((int)pair.Key)
                {
                    case (0):
                        {
                            this.db_manager.DynamicProperties.PutMaintenanceTimeInterval((int)pair.Value);
                        }
                        break;
                    case (1):
                        {
                            this.db_manager.DynamicProperties.PutAccountUpgradeCost((int)pair.Value);
                        }
                        break;
                    case (2):
                        {
                            this.db_manager.DynamicProperties.PutCreateAccountFee((int)pair.Value);
                        }
                        break;
                    case (3):
                        {
                            this.db_manager.DynamicProperties.PutTransactionFee((int)pair.Value);
                        }
                        break;
                    case (4):
                        {
                            this.db_manager.DynamicProperties.PutAssetIssueFee((int)pair.Value);
                        }
                        break;
                    case (5):
                        {
                            this.db_manager.DynamicProperties.PutWitnessPayPerBlock((int)pair.Value);
                        }
                        break;
                    case (6):
                        {
                            this.db_manager.DynamicProperties.PutWitnessStandbyAllowance((int)pair.Value);
                        }
                        break;
                    case (7):
                        {
                            this.db_manager.DynamicProperties.PutCreateNewAccountFeeInSystemContract((int)pair.Value);
                        }
                        break;
                    case (8):
                        {
                            this.db_manager.DynamicProperties.PutCreateNewAccountBandwidthRate((int)pair.Value);
                        }
                        break;
                    case (9):
                        {
                            this.db_manager.DynamicProperties.PutAllowCreationOfContracts((int)pair.Value);
                        }
                        break;
                    case (10):
                        {
                            if (this.db_manager.DynamicProperties.GetRemoveThePowerOfTheGr() == 0)
                            {
                                this.db_manager.DynamicProperties.PutRemoveThePowerOfTheGr((int)pair.Value);
                            }
                        }
                        break;
                    case (11):
                        {
                            this.db_manager.DynamicProperties.PutEnergyFee((int)pair.Value);
                        }
                        break;
                    case (12):
                        {
                            this.db_manager.DynamicProperties.PutExchangeCreateFee((int)pair.Value);
                        }
                        break;
                    case (13):
                        {
                            this.db_manager.DynamicProperties.PutMaxCpuTimeOfOneTx((int)pair.Value);
                        }
                        break;
                    case (14):
                        {
                            this.db_manager.DynamicProperties.PutAllowUpdateAccountName((int)pair.Value);
                        }
                        break;
                    case (15):
                        {
                            this.db_manager.DynamicProperties.PutAllowSameTokenName((int)pair.Value);
                        }
                        break;
                    case (16):
                        {
                            this.db_manager.DynamicProperties.PutAllowDelegateResource((int)pair.Value);
                        }
                        break;
                    case (17):
                        {
                            this.db_manager.DynamicProperties.PutTotalEnergyLimit((int)pair.Value);
                        }
                        break;
                    case (18):
                        {
                            this.db_manager.DynamicProperties.PutAllowTvmTransferTrc10((int)pair.Value);
                        }
                        break;
                    case (19):
                        {
                            this.db_manager.DynamicProperties.PutTotalEnergyLimit2((int)pair.Value);
                        }
                        break;
                    case (20):
                        {
                            if (this.db_manager.DynamicProperties.GetAllowMultiSign() == 0)
                            {
                                this.db_manager.DynamicProperties.PutAllowMultiSign((int)pair.Value);
                            }
                        }
                        break;
                    case (21):
                        {
                            if (this.db_manager.DynamicProperties.GetAllowAdaptiveEnergy() == 0)
                            {
                                this.db_manager.DynamicProperties.PutAllowAdaptiveEnergy((int)pair.Value);
                            }
                        }
                        break;
                    case (22):
                        {
                            this.db_manager.DynamicProperties.PutUpdateAccountPermissionFee((int)pair.Value);
                        }
                        break;
                    case (23):
                        {
                            this.db_manager.DynamicProperties.PutMultiSignFee((int)pair.Value);
                        }
                        break;
                    case (24):
                        {
                            this.db_manager.DynamicProperties.PutAllowProtoFilterNum((int)pair.Value);
                        }
                        break;
                    case (25):
                        {
                            this.db_manager.DynamicProperties.PutAllowAccountStateRoot((int)pair.Value);
                        }
                        break;
                    case (26):
                        {
                            this.db_manager.DynamicProperties.PutAllowTvmConstantinople((int)pair.Value);
                            this.db_manager.DynamicProperties.AddSystemContractAndSetPermission(48);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion
    }
}
