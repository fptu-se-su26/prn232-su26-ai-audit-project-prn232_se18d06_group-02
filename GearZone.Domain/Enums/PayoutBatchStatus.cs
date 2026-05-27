using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Enums
{
    public enum PayoutBatchStatus
    {
        Draft, 
        PendingApproval, 
        Approved, 
        Processing, 
        Completed, 
        PartialFailed, 
        OnHold
    }
}
