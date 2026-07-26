using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Admin
{
    public class AdminProductService : IAdminProductService
    {
        private const int ExportPageSize = 500;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public AdminProductService(
            IProductRepository productRepository, 
            IMapper mapper, 
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<PagedResult<AdminProductDto>> GetProductsAsync(AdminProductQueryDto queryDto)
        {
            var pagedProducts = await _productRepository.GetAdminProductsAsync(queryDto);

            var items = _mapper.Map<List<AdminProductDto>>(pagedProducts.Items);

            return new PagedResult<AdminProductDto>(items, pagedProducts.TotalCount, pagedProducts.PageNumber, pagedProducts.PageSize);
        }

        public async Task<AdminCsvFileDto> ExportProductsCsvAsync(
            AdminProductQueryDto queryDto,
            CancellationToken ct = default)
        {
            var exportQuery = CloneForExport(queryDto);
            var products = new List<AdminProductDto>();

            while (true)
            {
                ct.ThrowIfCancellationRequested();
                var page = await _productRepository.GetAdminProductsAsync(exportQuery);
                products.AddRange(_mapper.Map<List<AdminProductDto>>(page.Items));

                if (page.Items.Count == 0 || products.Count >= page.TotalCount)
                {
                    break;
                }

                exportQuery.PageNumber++;
            }

            return AdminCsvExporter.ExportProducts(products, DateTime.UtcNow);
        }

        public async Task<AdminProductStatsDto> GetProductStatsAsync()
        {
            return await _productRepository.GetAdminProductStatsAsync();
        }

        public async Task<AdminProductDetailDto?> GetProductDetailAsync(Guid id)
        {
            var product = await _productRepository.GetAdminProductDetailAsync(id);
            if (product == null)
                return null;

            return _mapper.Map<AdminProductDetailDto>(product);
        }

        public async Task<bool> DeleteProductAsync(Guid id, string reason)
        {
            var product = await _productRepository.GetByIdAsync(id, default, p => p.Store);
            if (product == null) return false;

            product.IsDeleted = true;
            product.StatusReason = reason;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            // Send email notification for deletion
            if (product.Store != null && !string.IsNullOrEmpty(product.Store.Email))
            {
                var subject = "Notice: Your Product has been Deleted";
                var body = $@"<h3>Attention {product.Store.StoreName},</h3>
                    <p>This is to inform you that your product <strong>{product.Name}</strong> has been <strong>deleted</strong> by the administrator.</p>
                    <p><strong>Reason:</strong><br/>{reason}</p><br/>
                    <p>Please contact support for more details regarding this action.</p><br/>
                    <p>Best regards,<br/>The GearZone Team</p>";
                await _emailService.SendAsync(product.Store.Email, subject, body);
            }

            return true;
        }

        public async Task<bool> BulkUpdateStatusAsync(List<Guid> productIds, ProductStatus status, string? reason = null)
        {
            if (productIds == null || !productIds.Any())
                return false;

            var success = false;
            foreach (var id in productIds)
            {
                var product = await _productRepository.GetByIdAsync(id, default, p => p.Store);
                if (product != null)
                {
                    product.Status = status;
                    product.StatusReason = reason;
                    await _productRepository.UpdateAsync(product);
                    success = true;

                    // Send email notification
                    if (Constant.ProductStatusSubject.ContainsKey(status) && product.Store != null && !string.IsNullOrEmpty(product.Store.Email))
                    {
                        var subject = Constant.ProductStatusSubject[status];
                        var body = string.Format(Constant.ProductStatusBody[status], product.Store.StoreName, product.Name, reason ?? "N/A");
                        await _emailService.SendAsync(product.Store.Email, subject, body);
                    }
                }
            }

            if (success)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return success;
        }

        private static AdminProductQueryDto CloneForExport(AdminProductQueryDto source) =>
            new()
            {
                SearchTerm = source.SearchTerm,
                Status = source.Status,
                CategoryId = source.CategoryId,
                BrandId = source.BrandId,
                StoreId = source.StoreId,
                MinPrice = source.MinPrice,
                MaxPrice = source.MaxPrice,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                OutOfStock = source.OutOfStock,
                AttributeOptionIds = source.AttributeOptionIds?.ToList() ?? new List<int>(),
                SortBy = source.SortBy,
                SortDirection = source.SortDirection,
                PageNumber = 1,
                PageSize = ExportPageSize
            };
    }
}
