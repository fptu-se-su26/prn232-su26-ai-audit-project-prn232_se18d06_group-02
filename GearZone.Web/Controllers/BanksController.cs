using GearZone.Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BanksController : ControllerBase
    {
        private readonly IBankCatalogService _bankCatalogService;

        public BanksController(IBankCatalogService bankCatalogService)
        {
            _bankCatalogService = bankCatalogService;
        }

        [HttpGet]
        public IActionResult GetSupportedBanks()
        {
            return Ok(_bankCatalogService.GetSupportedBanks());
        }
    }
}
