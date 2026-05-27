using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GearZone.Web.Pages.Checkout
{
    [Authorize]
    public class SuccessModel : PageModel
    {
        private readonly IOrderRepository _orderRepository;

        public SuccessModel(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public Order Order { get; set; } = null!;
        public string PaymentMethodName { get; set; } = "COD";
        public Guid? TrackSubOrderId { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Public/Auth/Login");

            Order = await _orderRepository.Query()
                .Include(o => o.Payments)
                .Include(o => o.SubOrders)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (Order == null)
            {
                return RedirectToPage("/Index");
            }

            var payment = Order.Payments.FirstOrDefault();
            if (payment != null)
            {
                PaymentMethodName = payment.Method switch
                {
                    PaymentMethod.COD => "Cash on Delivery",
                    PaymentMethod.PayOS => "PayOS",
                    _ => "Other"
                };
            }

            TrackSubOrderId = Order.SubOrders
                .OrderBy(x => x.CreatedAt)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefault();

            return Page();
        }
    }
}
