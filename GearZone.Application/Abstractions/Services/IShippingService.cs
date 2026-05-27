using GearZone.Application.Features.Shipping.Dtos;
using GearZone.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public interface IShippingService
    {
        Task<ShippingFeeCalculationResponseDto> CalculateShippingFeeAsync(double destLat, double destLng, List<CartItem> items);
    }
}
