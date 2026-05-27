using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class PayoutItem : Entity<Guid>
    {
        public Guid PayoutTransactionId { get; set; }
        public Guid SubOrderId { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetAmount { get; set; }
        public bool IsExcluded { get; set; } = false;
        public string? ExcludeReason { get; set; }

        public PayoutTransaction Transaction { get; set; } = null!;
        public SubOrder SubOrder { get; set; } = null!;
    }
}
