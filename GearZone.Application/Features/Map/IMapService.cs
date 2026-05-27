using GearZone.Application.Common.Dtos;
using GearZone.Application.Features.Map.Dtos;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Map;

public interface IMapService
{
    Task<GoongAutocompleteResponse?> GetAutocompleteAsync(string input);
    Task<AddressDetailDto?> GetAddressDetailAsync(string placeId);
    Task<AddressDetailDto?> GetReverseGeocodeAsync(double lat, double lng);
}
