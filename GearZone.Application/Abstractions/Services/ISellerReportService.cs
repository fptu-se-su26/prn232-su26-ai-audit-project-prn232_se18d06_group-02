using System.Threading;
using System.Threading.Tasks;
using GearZone.Application.Features.Seller.Dtos;

namespace GearZone.Application.Abstractions.Services
{
    /// <summary>
    /// Analytics/reporting for a store owner. Every report is scoped to the caller's
    /// store and to a resolved date range, and compares against the immediately
    /// preceding period of the same length.
    /// </summary>
    public interface ISellerReportService
    {
        Task<SalesReportDto> GetSalesReportAsync(string ownerUserId, SellerReportQueryDto query, CancellationToken ct = default);
        Task<ProductPerformanceReportDto> GetProductPerformanceAsync(string ownerUserId, SellerReportQueryDto query, CancellationToken ct = default);
        Task<OperationsReportDto> GetOperationsReportAsync(string ownerUserId, SellerReportQueryDto query, CancellationToken ct = default);
        Task<CustomerReportDto> GetCustomerReportAsync(string ownerUserId, SellerReportQueryDto query, CancellationToken ct = default);
        Task<MarketingReportDto> GetMarketingReportAsync(string ownerUserId, SellerReportQueryDto query, CancellationToken ct = default);
        Task<ReviewsReportDto> GetReviewsReportAsync(string ownerUserId, SellerReportQueryDto query, CancellationToken ct = default);

        /// <summary>Builds a CSV export of the sales time series for the given range.</summary>
        Task<string> GetSalesCsvAsync(string ownerUserId, SellerReportQueryDto query, CancellationToken ct = default);
    }
}
