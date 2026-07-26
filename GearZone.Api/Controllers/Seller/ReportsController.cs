using System.Text;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Api.Controllers.Seller;

[Authorize(Roles = "Store Owner")]
[Route("api/seller/reports")]
[ApiController]
public class ReportsController : BaseApiController
{
    private readonly ISellerReportService _reportService;

    public ReportsController(ISellerReportService reportService)
    {
        _reportService = reportService;
    }

    // GET /api/seller/reports/sales?range=30d&granularity=day&from=&to=
    [HttpGet("sales")]
    public async Task<IActionResult> Sales([FromQuery] SellerReportQueryDto query, CancellationToken ct)
        => OkResponse(await _reportService.GetSalesReportAsync(CurrentUserId!, query, ct));

    // GET /api/seller/reports/sales/export -> text/csv
    [HttpGet("sales/export")]
    public async Task<IActionResult> ExportSales([FromQuery] SellerReportQueryDto query, CancellationToken ct)
    {
        var csv = await _reportService.GetSalesCsvAsync(CurrentUserId!, query, ct);
        var fileName = $"sales-report-{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
    }

    // GET /api/seller/reports/products
    [HttpGet("products")]
    public async Task<IActionResult> Products([FromQuery] SellerReportQueryDto query, CancellationToken ct)
        => OkResponse(await _reportService.GetProductPerformanceAsync(CurrentUserId!, query, ct));

    // GET /api/seller/reports/operations
    [HttpGet("operations")]
    public async Task<IActionResult> Operations([FromQuery] SellerReportQueryDto query, CancellationToken ct)
        => OkResponse(await _reportService.GetOperationsReportAsync(CurrentUserId!, query, ct));

    // GET /api/seller/reports/customers
    [HttpGet("customers")]
    public async Task<IActionResult> Customers([FromQuery] SellerReportQueryDto query, CancellationToken ct)
        => OkResponse(await _reportService.GetCustomerReportAsync(CurrentUserId!, query, ct));

    // GET /api/seller/reports/marketing
    [HttpGet("marketing")]
    public async Task<IActionResult> Marketing([FromQuery] SellerReportQueryDto query, CancellationToken ct)
        => OkResponse(await _reportService.GetMarketingReportAsync(CurrentUserId!, query, ct));

    // GET /api/seller/reports/reviews
    [HttpGet("reviews")]
    public async Task<IActionResult> Reviews([FromQuery] SellerReportQueryDto query, CancellationToken ct)
        => OkResponse(await _reportService.GetReviewsReportAsync(CurrentUserId!, query, ct));
}
