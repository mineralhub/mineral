using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Witness
{
    public enum BlockProductionCondition
    {
        PRODUCED,
        UNELECTED,
        NOT_MY_TURN,
        NOT_SYNCED,
        NOT_TIME_YET,
        NO_PRIVATE_KEY,
        WITNESS_PERMISSION_ERROR,
        LOW_PARTICIPATION,
        LAG,
        CONSECUTIVE,
        TIME_OUT,
        BACKUP_STATUS_IS_NOT_MASTER,
        DUP_WITNESS,
        EXCEPTION_PRODUCING_BLOCK
    }
}
