using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Api.Controllers;

[AllowAnonymous]
public class ConfigController : BaseApiController
{
    private readonly IConfiguration _configuration;

    public ConfigController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // GET /api/config/map-keys
    // Returns public map configuration keys needed by the React frontend.
    // GOONG_API_KEY is intentionally excluded — it must remain server-side.
    [HttpGet("map-keys")]
    public IActionResult MapKeys()
    {
        return OkResponse(new
        {
            goongMapKey = _configuration["GOONG_MAP_KEY"],
            mapBoxStyle = _configuration["MAPBOX_STYLE"]
        });
    }
}
