using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Cart.DTOs;
using GearZone.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Application.Features.Cart
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CartService(
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IProductVariantRepository productVariantRepository,
            IUnitOfWork unitOfWork)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productVariantRepository = productVariantRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> AddToCartAsync(string userId, Guid variantId, int quantity, bool isBuyNow = false)
        {
            var variant = await _productVariantRepository.GetByIdAsync(variantId);
            if (variant == null)
                throw new KeyNotFoundException("Product not found.");

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0.");

            var cart = await _cartRepository.Query().FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Domain.Entities.Cart { Id = Guid.NewGuid(), UserId = userId, UpdatedAt = DateTime.UtcNow };
                await _cartRepository.AddAsync(cart);
            }

            var cartItem = await _cartItemRepository.Query().FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.VariantId == variantId);
            if (cartItem != null)
            {
                if (isBuyNow)
                {
                    cartItem.Quantity = quantity;
                    if (cartItem.Quantity > variant.StockQuantity)
                        cartItem.Quantity = variant.StockQuantity;
                }
                else
                {
                    int newTotalQuantity = cartItem.Quantity + quantity;
                    if (newTotalQuantity > variant.StockQuantity)
                        throw new InvalidOperationException($"Insufficient stock. Maximum available: {variant.StockQuantity}.");
                    
                    cartItem.Quantity = newTotalQuantity;
                }
                cart.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                if (quantity > variant.StockQuantity)
                    throw new InvalidOperationException($"Insufficient stock. Maximum available: {variant.StockQuantity}.");

                cartItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    Cart = cart,
                    VariantId = variantId,
                    Quantity = quantity
                };
                cart.UpdatedAt = DateTime.UtcNow;
                await _cartItemRepository.AddAsync(cartItem);
            }

            await _unitOfWork.SaveChangesAsync();
            return cartItem.Id;
        }

        public async Task<CartDto?> GetCartAsync(string userId)
        {
            var cart = await _cartRepository.GetDetailedCartAsync(userId);
            if (cart == null)
            {
                return new CartDto { UserId = userId, StoreGroups = new List<StoreCartGroupDto>(), TotalPrice = 0 };
            }

            var storeGroups = cart.Items
                .GroupBy(i => i.Variant.Product.Store)
                .Select(g => new StoreCartGroupDto
                {
                    StoreId = g.Key.Id,
                    StoreName = g.Key.StoreName,
                    Items = g.Select(i => new CartItemDto
                    {
                        Id = i.Id,
                        VariantId = i.VariantId,
                        ProductName = i.Variant.Product.Name,
                        ProductSlug = i.Variant.Product.Slug,
                        ImageUrl = i.Variant.Product.Images.OrderByDescending(img => img.IsPrimary).FirstOrDefault()?.ImageUrl,
                        VariantName = i.Variant.VariantName,
                        Price = i.Variant.Price,
                        Quantity = i.Quantity,
                        StockQuantity = i.Variant.StockQuantity
                    }).ToList(),
                    StoreSubtotal = g.Sum(i => i.Quantity * i.Variant.Price)
                }).ToList();

            var total = storeGroups.Sum(g => g.StoreSubtotal);

            return new CartDto
            {
                Id = cart.Id,
                UserId = userId,
                StoreGroups = storeGroups,
                TotalPrice = total
            };
        }

        public async Task RemoveCartItemAsync(Guid cartItemId, string userId)
        {
            var cartItem = await _cartItemRepository.GetByIdAsync(cartItemId);
            if (cartItem == null)
                throw new KeyNotFoundException("Product not found in cart.");

            var cart = await _cartRepository.GetByIdAsync(cartItem.CartId);
            if (cart == null || cart.UserId != userId)
                throw new UnauthorizedAccessException("Unauthorized access.");

            await _cartItemRepository.DeleteAsync(cartItem);
            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(cart);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateCartItemQuantityAsync(Guid cartItemId, int newQuantity, string userId)
        {
            var cartItem = await _cartItemRepository.GetByIdAsync(cartItemId);
            if (cartItem == null)
                throw new KeyNotFoundException("Product not found in cart.");

            var cart = await _cartRepository.GetByIdAsync(cartItem.CartId);
            if (cart == null || cart.UserId != userId)
                throw new UnauthorizedAccessException("Unauthorized access.");

            if (newQuantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0.");

            var variant = await _productVariantRepository.GetByIdAsync(cartItem.VariantId);
            if(variant == null)
                 throw new KeyNotFoundException("Product not found.");

            if (newQuantity > variant.StockQuantity)
                throw new InvalidOperationException($"Insufficient stock. Maximum available: {variant.StockQuantity}.");

            cartItem.Quantity = newQuantity;
            cart.UpdatedAt = DateTime.UtcNow;
            await _cartItemRepository.UpdateAsync(cartItem);
            await _cartRepository.UpdateAsync(cart);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> GetCartItemsCountAsync(string userId)
        {
            var cart = await _cartRepository.Query()
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart?.Items.Sum(i => i.Quantity) ?? 0;
        }

        public async Task ClearCartItemsAsync(IEnumerable<Guid> cartItemIds, CancellationToken ct = default)
        {
            await _cartItemRepository.DeleteRangeByIdsAsync(cartItemIds, ct);
        }
    }
}
