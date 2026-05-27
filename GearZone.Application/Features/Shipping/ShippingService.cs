using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Shipping.Dtos;
using GearZone.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Shipping
{
    public class ShippingService : IShippingService
    {
        private readonly IGoongService _goongService;
        private readonly IMemoryCache _cache;

        private const decimal BaseFee = 15000;
        private const decimal PricePerKm = 500;
        private const decimal MinFee = 15000;
        private const decimal MaxFee = 500000;
        private const decimal FreeShippingThreshold = 200000;

        public ShippingService(IGoongService goongService, IMemoryCache cache)
        {
            _goongService = goongService;
            _cache = cache;
        }

        public async Task<ShippingFeeCalculationResponseDto> CalculateShippingFeeAsync(double destLat, double destLng, List<CartItem> items)
        {
            var response = new ShippingFeeCalculationResponseDto();
            var storeGroups = items.GroupBy(i => i.Variant.Product.StoreId);

            foreach (var group in storeGroups)
            {
                var storeId = group.Key;
                var storeItems = group.ToList();
                var store = storeItems.First().Variant.Product.Store;
                var subtotalOriginal = storeItems.Sum(i => i.Quantity * i.Variant.Price);

                var storeFeeDto = new StoreShippingFeeDto
                {
                    StoreId = storeId,
                    StoreName = store?.StoreName ?? "Unknown Store",
                    IsFreeShipping = subtotalOriginal >= FreeShippingThreshold
                };

                if (storeFeeDto.IsFreeShipping)
                {
                    storeFeeDto.ShippingFee = 0;
                    storeFeeDto.DistanceKm = 0; // Or calculate if needed for display
                }
                else
                {
                    if (store != null && store.Latitude != null && store.Longitude != null)
                    {
                        var distance = await GetCachedDistanceAsync((double)store.Latitude, (double)store.Longitude, destLat, destLng);
                        if (distance != null)
                        {
                            double distVal = (double)distance;
                            storeFeeDto.DistanceKm = distVal;
                            decimal calculatedFee = BaseFee + (decimal)(distVal * (double)PricePerKm);
                            
                            // Apply Min/Max constraints
                            if (calculatedFee < MinFee) calculatedFee = MinFee;
                            if (calculatedFee > MaxFee) calculatedFee = MaxFee;

                            storeFeeDto.ShippingFee = Math.Round(calculatedFee, 0);
                        }
                        else
                        {
                            // Fallback if Goong fails or distance not found
                            storeFeeDto.ShippingFee = MinFee;
                        }
                    }
                    else
                    {
                        // Fallback if Store coordinates missing
                        storeFeeDto.ShippingFee = MinFee;
                    }
                }

                response.StoreFees.Add(storeFeeDto);
                response.TotalShippingFee += storeFeeDto.ShippingFee;
            }

            return response;
        }

        private async Task<double?> GetCachedDistanceAsync(double oLat, double oLng, double dLat, double dLng)
        {
            string cacheKey = $"dist_{oLat:F4}_{oLng:F4}_{dLat:F4}_{dLng:F4}";
            if (!_cache.TryGetValue(cacheKey, out double? distance))
            {
                distance = await _goongService.GetDistanceAsync(oLat, oLng, dLat, dLng);
                if (distance != null)
                {
                    _cache.Set(cacheKey, distance, TimeSpan.FromHours(24));
                }
            }
            return distance;
        }
    }
}
