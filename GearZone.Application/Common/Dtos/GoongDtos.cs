using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GearZone.Application.Common.Dtos
{
    public class GoongAutocompleteResponse
    {
        [JsonPropertyName("predictions")]
        public List<GoongPrediction> Predictions { get; set; } = new();

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class GoongPrediction
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("place_id")]
        public string PlaceId { get; set; } = string.Empty;

        [JsonPropertyName("structured_formatting")]
        public GoongStructuredFormatting StructuredFormatting { get; set; } = new();
    }

    public class GoongStructuredFormatting
    {
        [JsonPropertyName("main_text")]
        public string MainText { get; set; } = string.Empty;

        [JsonPropertyName("secondary_text")]
        public string SecondaryText { get; set; } = string.Empty;
    }

    public class GoongPlaceDetailResponse
    {
        [JsonPropertyName("result")]
        public GoongPlaceResult Result { get; set; } = new();

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class GoongPlaceResult
    {
        [JsonPropertyName("formatted_address")]
        public string FormattedAddress { get; set; } = string.Empty;

        [JsonPropertyName("geometry")]
        public GoongGeometry Geometry { get; set; } = new();

        [JsonPropertyName("address_components")]
        public List<GoongAddressComponent> AddressComponents { get; set; } = new();
    }

    public class GoongGeometry
    {
        [JsonPropertyName("location")]
        public GoongLocation Location { get; set; } = new();
    }

    public class GoongLocation
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }
    }

    public class GoongAddressComponent
    {
        [JsonPropertyName("long_name")]
        public string LongName { get; set; } = string.Empty;

        [JsonPropertyName("short_name")]
        public string ShortName { get; set; } = string.Empty;

        [JsonPropertyName("types")]
        public List<string> Types { get; set; } = new();
    }

    public class GoongGeocodeResponse
    {
        [JsonPropertyName("results")]
        public List<GoongPlaceResult> Results { get; set; } = new();

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class GoongDistanceMatrixResponse
    {
        [JsonPropertyName("rows")]
        public List<GoongDistanceRow> Rows { get; set; } = new();

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class GoongDistanceRow
    {
        [JsonPropertyName("elements")]
        public List<GoongDistanceElement> Elements { get; set; } = new();
    }

    public class GoongDistanceElement
    {
        [JsonPropertyName("distance")]
        public GoongDistanceValue Distance { get; set; } = new();

        [JsonPropertyName("duration")]
        public GoongDistanceValue Duration { get; set; } = new();

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class GoongDistanceValue
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public int ValueInMeters { get; set; } // meters
    }
}
