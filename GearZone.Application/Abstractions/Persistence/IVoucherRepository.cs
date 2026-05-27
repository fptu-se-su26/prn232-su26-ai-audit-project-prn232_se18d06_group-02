using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IVoucherRepository : IRepository<Voucher, Guid>
    {
        Task<PagedResult<Voucher>> GetPaginatedAdminVouchersAsync(AdminVoucherQueryDto query);
        Task<AdminVoucherSummaryDto> GetAdminVoucherSummaryAsync();
        Task<PagedResult<Voucher>> GetPaginatedSellerVouchersAsync(Guid storeId, SellerVoucherQueryDto query);
        Task<SellerVoucherSummaryDto> GetSellerVoucherSummaryAsync(Guid storeId);
        Task<Voucher?> GetByCodeAsync(string code);
        Task<List<Voucher>> GetAvailableVouchersAsync(VoucherType type);
    }
}
