using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Map;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GearZone.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MapsController : ControllerBase
    {
        private readonly IMapService _mapService;

        public MapsController(IMapService mapService)
        {
            _mapService = mapService;
        }

        [HttpGet("autocomplete")]
        public async Task<IActionResult> Autocomplete([FromQuery] string input)
        {
            var result = await _mapService.GetAutocompleteAsync(input);
            if (result == null) return BadRequest();
            return Ok(result);
        }

        [HttpGet("place-detail")]
        public async Task<IActionResult> PlaceDetail([FromQuery] string placeId)
        {
            var result = await _mapService.GetAddressDetailAsync(placeId);
            if (result == null) return BadRequest();
            return Ok(result);
        }

        [HttpGet("reverse-geocode")]
        public async Task<IActionResult> ReverseGeocode([FromQuery] double lat, [FromQuery] double lng)
        {
            var result = await _mapService.GetReverseGeocodeAsync(lat, lng);
            if (result == null) return BadRequest();
            return Ok(result);
        }
    }
}
