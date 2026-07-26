using System;
using System.Threading;
using System.Threading.Tasks;
using GearZone.Application.Features.Seller.Dtos;

namespace GearZone.Application.Abstractions.Services
{
    /// <summary>
    /// Bulk product import for sellers via an Excel (.xlsx) file. Implemented in the
    /// Infrastructure layer (uses ClosedXML); orchestrates validation + creation through
    /// <see cref="ISellerProductService"/>.
    /// </summary>
    public interface IProductImportService
    {
        /// <summary>Builds a fill-in .xlsx template (with the store's valid categories/brands).</summary>
        Task<byte[]> GenerateTemplateAsync(CancellationToken ct = default);

        /// <summary>Parses + validates the file and returns a per-row preview. Writes nothing.</summary>
        Task<ProductImportPreviewDto> PreviewAsync(byte[] fileBytes, Guid storeId, CancellationToken ct = default);

        /// <summary>Re-validates and creates the valid products, skipping (and reporting) invalid rows.</summary>
        Task<ProductImportResultDto> ImportAsync(byte[] fileBytes, Guid storeId, string userId, CancellationToken ct = default);
    }
}
