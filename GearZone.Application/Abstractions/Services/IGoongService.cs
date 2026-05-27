using GearZone.Application.Common.Dtos;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public interface IGoongService
    {
        Task<GoongAutocompleteResponse?> GetAutocompleteAsync(string input);
        Task<GoongPlaceDetailResponse?> GetPlaceDetailAsync(string placeId);
        Task<GoongGeocodeResponse?> GetReverseGeocodeAsync(double lat, double lng);
        Task<double?> GetDistanceAsync(double originLat, double originLng, double destinationLat, double destinationLng);
    }
}
