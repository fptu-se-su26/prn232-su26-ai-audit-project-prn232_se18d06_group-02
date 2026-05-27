using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Dtos;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace GearZone.Infrastructure.External
{
    public class GoongService : IGoongService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GoongService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GOONG_API_KEY"] ?? throw new InvalidOperationException("GOONG_API_KEY not found in configuration.");
        }

        public async Task<GoongAutocompleteResponse?> GetAutocompleteAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            var url = $"https://rsapi.goong.io/Place/AutoComplete?api_key={_apiKey}&input={Uri.EscapeDataString(input)}";
            
            try
            {
                var response = await _httpClient.GetFromJsonAsync<GoongAutocompleteResponse>(url);
                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<GoongPlaceDetailResponse?> GetPlaceDetailAsync(string placeId)
        {
            if (string.IsNullOrWhiteSpace(placeId)) return null;

            var url = $"https://rsapi.goong.io/Place/Detail?api_key={_apiKey}&place_id={placeId}";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<GoongPlaceDetailResponse>(url);
                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<GoongGeocodeResponse?> GetReverseGeocodeAsync(double lat, double lng)
        {
            var latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var lngStr = lng.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var url = $"https://rsapi.goong.io/Geocode?api_key={_apiKey}&latlng={latStr},{lngStr}";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<GoongGeocodeResponse>(url);
                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<double?> GetDistanceAsync(double originLat, double originLng, double destinationLat, double destinationLng)
        {
            var oLatStr = originLat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var oLngStr = originLng.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var dLatStr = destinationLat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var dLngStr = destinationLng.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var url = $"https://rsapi.goong.io/DistanceMatrix?origins={oLatStr},{oLngStr}&destinations={dLatStr},{dLngStr}&vehicle=car&api_key={_apiKey}";

            try
            {
                var response = await _httpClient.GetFromJsonAsync<GoongDistanceMatrixResponse>(url);
                if (response != null && response.Rows.Count > 0 && response.Rows[0].Elements.Count > 0)
                {
                    var element = response.Rows[0].Elements[0];
                    if (element.Status == "OK")
                    {
                        return element.Distance.ValueInMeters / 1000.0; // Convert meters to km
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
