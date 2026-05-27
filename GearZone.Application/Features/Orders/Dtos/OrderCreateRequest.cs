using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GearZone.Application.Features.Orders.Dtos
{
    public class OrderCreateRequest
    {
        // Shipping info
        public string ReceiverName { get; set; } = string.Empty;

        public string ReceiverPhone { get; set; } = string.Empty;

        public string ShippingAddress { get; set; } = string.Empty;

        public string? Note { get; set; }

        // Items
        public List<OrderItemCreateRequest> Items { get; set; } = new();

        // PayOS
        public string ReturnUrl { get; set; } = string.Empty;

        public string CancelUrl { get; set; } = string.Empty;

        public string Description { get; set; } = "Thanh toan don hang";
    }

    public class OrderItemCreateRequest
    {
        public Guid VariantId { get; set; }

        public int Quantity { get; set; }
    }
}
