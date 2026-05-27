using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Enums
{
    public enum PayoutTransactionStatus
    {
        Queued,
        Processing,
        Success,
        Failed,
        ManualRequired,
        Excluded
    }
}
