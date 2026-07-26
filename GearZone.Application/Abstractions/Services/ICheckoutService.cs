using System;
using System.Collections.Generic;
using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public interface ICheckoutService
    {
        Task<CheckoutResponseDto> ProcessCheckoutAsync(string userId, CheckoutRequestDto request, CancellationToken ct = default);
        Task<List<CartItem>> GetCheckoutItemsAsync(string userId, List<Guid> cartItemIds, CancellationToken ct = default);
        Task<CheckoutQuoteDto> GetQuoteAsync(
            string userId,
            CheckoutQuoteRequestDto request,
            CancellationToken ct = default);
    }
}
