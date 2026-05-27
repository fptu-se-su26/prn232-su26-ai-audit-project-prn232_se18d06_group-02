using GearZone.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GearZone.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var cartItemId = await _cartService.AddToCartAsync(userId, request.VariantId, request.Quantity, request.IsBuyNow);
                var cartCount = await _cartService.GetCartItemsCountAsync(userId);
                return Ok(new { message = "Added to cart successfully.", cartItemId, cartCount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _cartService.UpdateCartItemQuantityAsync(request.CartItemId, request.Quantity, userId);
                return Ok(new { message = "Quantity updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cart = await _cartService.GetCartAsync(userId);
            return Ok(new { success = true, data = cart });
        }

        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> RemoveItem(Guid cartItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _cartService.RemoveCartItemAsync(cartItemId, userId);
                return Ok(new { message = "Product removed from cart." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class AddToCartRequest
    {
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
        public bool IsBuyNow { get; set; }
    }

    public class UpdateQuantityRequest
    {
        public Guid CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}
