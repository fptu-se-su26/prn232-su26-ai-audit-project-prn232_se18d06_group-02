using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Application.Features.Seller
{
    public class SellerVoucherService : ISellerVoucherService
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SellerVoucherService(
            IVoucherRepository voucherRepository,
            IStoreRepository storeRepository,
            IUnitOfWork unitOfWork)
        {
            _voucherRepository = voucherRepository;
            _storeRepository = storeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<SellerVoucherDto>> GetPaginatedVouchersAsync(string ownerUserId, SellerVoucherQueryDto query)
        {
            query ??= new SellerVoucherQueryDto();

            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return new PagedResult<SellerVoucherDto>(new List<SellerVoucherDto>(), 0, 1, 10);
            }

            var store = await GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null)
            {
                return new PagedResult<SellerVoucherDto>(new List<SellerVoucherDto>(), 0, 1, 10);
            }

            query.PageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            query.PageSize = query.PageSize < 1 ? 10 : query.PageSize;

            var pagedVouchers = await _voucherRepository.GetPaginatedSellerVouchersAsync(store.Id, query);
            var items = pagedVouchers.Items.Select(MapToDto).ToList();

            return new PagedResult<SellerVoucherDto>(items, pagedVouchers.TotalCount, query.PageNumber, query.PageSize);
        }

        public async Task<SellerVoucherSummaryDto> GetVoucherSummaryAsync(string ownerUserId)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return new SellerVoucherSummaryDto();
            }

            var store = await GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null)
            {
                return new SellerVoucherSummaryDto();
            }

            return await _voucherRepository.GetSellerVoucherSummaryAsync(store.Id);
        }

        public async Task<SellerVoucherDto?> GetVoucherByIdAsync(string ownerUserId, Guid voucherId)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId) || voucherId == Guid.Empty)
            {
                return null;
            }

            var store = await GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null)
            {
                return null;
            }

            var voucher = await _voucherRepository.Query()
                .AsNoTracking()
                .Include(v => v.Category)
                .Include(v => v.Usages)
                .FirstOrDefaultAsync(v =>
                    v.Id == voucherId &&
                    v.StoreId == store.Id &&
                    v.Scope == VoucherScope.Seller);

            return voucher == null ? null : MapToDto(voucher);
        }

        public async Task<(bool Success, string? Error)> CreateVoucherAsync(string ownerUserId, SellerCreateVoucherDto dto)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return (false, "Please login again.");
            }

            var store = await GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null)
            {
                return (false, "Store not found.");
            }

            var validationError = ValidateVoucherInput(dto);
            if (!string.IsNullOrWhiteSpace(validationError))
            {
                return (false, validationError);
            }

            var code = dto.Code.ToUpper().Trim();
            var codeExists = await _voucherRepository.Query().AnyAsync(v => v.Code == code);
            if (codeExists)
            {
                return (false, "Voucher code already exists.");
            }

            var voucherType = ParseVoucherType(dto.Type);
            var discountType = ParseDiscountType(dto.DiscountType);
            var voucher = new Voucher
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = dto.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                Type = voucherType,
                Scope = VoucherScope.Seller,
                StoreId = store.Id,
                CategoryId = voucherType == VoucherType.OrderDiscount ? dto.CategoryId : null,
                DiscountType = discountType,
                DiscountValue = dto.DiscountValue,
                MaxDiscount = discountType == DiscountType.FixedAmount ? null : dto.MaxDiscount,
                MinOrderAmount = dto.MinOrderAmount,
                UsageLimit = dto.UsageLimit,
                UsedCount = 0,
                MaxUsagePerUser = dto.MaxUsagePerUser,
                StartAt = dto.StartAt,
                EndAt = dto.EndAt,
                IsActive = dto.IsVisible,
                Status = dto.IsVisible ? DetermineStatus(dto.StartAt, dto.EndAt) : VoucherStatus.Disabled,
                CreatedAt = DateTime.Now
            };

            await _voucherRepository.AddAsync(voucher);
            await _unitOfWork.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> UpdateVoucherAsync(string ownerUserId, Guid voucherId, SellerUpdateVoucherDto dto)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return (false, "Please login again.");
            }

            var store = await GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null)
            {
                return (false, "Store not found.");
            }

            var voucher = await _voucherRepository.Query()
                .FirstOrDefaultAsync(v =>
                    v.Id == voucherId &&
                    v.StoreId == store.Id &&
                    v.Scope == VoucherScope.Seller);

            if (voucher == null)
            {
                return (false, "Voucher not found.");
            }

            if (dto.UsageLimit < voucher.UsedCount)
            {
                return (false, "Usage limit cannot be lower than reserved or redeemed usages.");
            }

            var validationError = ValidateVoucherInput(dto);
            if (!string.IsNullOrWhiteSpace(validationError))
            {
                return (false, validationError);
            }

            var code = dto.Code.ToUpper().Trim();
            var codeExists = await _voucherRepository.Query().AnyAsync(v => v.Code == code && v.Id != voucherId);
            if (codeExists)
            {
                return (false, "Voucher code already exists.");
            }

            var discountType = ParseDiscountType(dto.DiscountType);
            var voucherType = ParseVoucherType(dto.Type);

            voucher.Code = code;
            voucher.Name = dto.Name.Trim();
            voucher.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            voucher.Type = voucherType;
            voucher.Scope = VoucherScope.Seller;
            voucher.StoreId = store.Id;
            voucher.CategoryId = voucherType == VoucherType.OrderDiscount ? dto.CategoryId : null;
            voucher.DiscountType = discountType;
            voucher.DiscountValue = dto.DiscountValue;
            voucher.MaxDiscount = discountType == DiscountType.FixedAmount ? null : dto.MaxDiscount;
            voucher.MinOrderAmount = dto.MinOrderAmount;
            voucher.UsageLimit = dto.UsageLimit;
            voucher.MaxUsagePerUser = dto.MaxUsagePerUser;
            voucher.StartAt = dto.StartAt;
            voucher.EndAt = dto.EndAt;
            voucher.IsActive = dto.IsVisible;
            voucher.Status = dto.IsVisible ? DetermineStatus(dto.StartAt, dto.EndAt) : VoucherStatus.Disabled;

            await _voucherRepository.UpdateAsync(voucher);
            await _unitOfWork.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> ToggleVoucherStatusAsync(string ownerUserId, Guid voucherId)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return (false, "Please login again.");
            }

            var store = await GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null)
            {
                return (false, "Store not found.");
            }

            var voucher = await _voucherRepository.Query()
                .FirstOrDefaultAsync(v =>
                    v.Id == voucherId &&
                    v.StoreId == store.Id &&
                    v.Scope == VoucherScope.Seller);

            if (voucher == null)
            {
                return (false, "Voucher not found.");
            }

            if (voucher.Status == VoucherStatus.Disabled)
            {
                voucher.IsActive = true;
                voucher.Status = DetermineStatus(voucher.StartAt, voucher.EndAt);
            }
            else
            {
                voucher.IsActive = false;
                voucher.Status = VoucherStatus.Disabled;
            }

            await _voucherRepository.UpdateAsync(voucher);
            await _unitOfWork.SaveChangesAsync();
            return (true, null);
        }

        private async Task<Store?> GetStoreByOwnerIdAsync(string ownerUserId)
        {
            return await _storeRepository.GetStoreByOwnerIdAsync(ownerUserId);
        }

        private static VoucherStatus DetermineStatus(DateTime startAt, DateTime endAt)
        {
            var now = DateTime.Now;
            if (now < startAt) return VoucherStatus.Upcoming;
            if (now <= endAt) return VoucherStatus.Active;
            return VoucherStatus.Expired;
        }

        private static DiscountType ParseDiscountType(string value)
        {
            if (string.Equals(value, "Fixed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "FixedAmount", StringComparison.OrdinalIgnoreCase))
            {
                return DiscountType.FixedAmount;
            }

            return DiscountType.Percent;
        }

        private static VoucherType ParseVoucherType(string value)
        {
            return string.Equals(value, "Shipping", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "ShippingDiscount", StringComparison.OrdinalIgnoreCase)
                ? VoucherType.ShippingDiscount
                : VoucherType.OrderDiscount;
        }

        private static string? ValidateVoucherInput(SellerCreateVoucherDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Code))
            {
                return "Voucher name and code are required.";
            }

            if (dto.UsageLimit < 1)
            {
                return "Usage limit must be at least 1.";
            }

            if (dto.MaxUsagePerUser < 1 || dto.MaxUsagePerUser > 1000)
            {
                return "Max usage per user must be between 1 and 1000.";
            }

            if (dto.DiscountValue <= 0)
            {
                return "Discount value must be greater than 0.";
            }

            if (dto.MinOrderAmount < 0)
            {
                return "Minimum spend cannot be negative.";
            }

            if (dto.EndAt <= dto.StartAt)
            {
                return "Expiration date must be later than launch date.";
            }

            var discountType = ParseDiscountType(dto.DiscountType);
            if (discountType == DiscountType.Percent && dto.DiscountValue > 100)
            {
                return "Discount percentage must be less than or equal to 100%.";
            }

            if (discountType == DiscountType.Percent && dto.MaxDiscount.HasValue && dto.MaxDiscount.Value <= 0)
            {
                return "Maximum discount must be greater than 0.";
            }

            if (discountType == DiscountType.FixedAmount && dto.MinOrderAmount <= dto.DiscountValue)
            {
                return "Minimum spend must be greater than fixed discount.";
            }

            return null;
        }

        private static SellerVoucherDto MapToDto(Voucher voucher)
        {
            return new SellerVoucherDto
            {
                Id = voucher.Id,
                Code = voucher.Code,
                Name = voucher.Name,
                Description = voucher.Description,
                Type = voucher.Type,
                DiscountType = voucher.DiscountType,
                DiscountValue = voucher.DiscountValue,
                MaxDiscount = voucher.MaxDiscount,
                MinOrderAmount = voucher.MinOrderAmount,
                UsageLimit = voucher.UsageLimit,
                MaxUsagePerUser = voucher.MaxUsagePerUser,
                UsedCount = voucher.UsedCount,
                RedeemedCount = voucher.Usages.Count(x =>
                    x.Status == VoucherUsageStatus.Redeemed),
                TotalSavedAmount = voucher.Usages
                    .Where(x => x.Status == VoucherUsageStatus.Redeemed)
                    .Sum(x => x.DiscountAmount),
                CategoryId = voucher.CategoryId,
                CategoryName = voucher.Category?.Name,
                StartAt = voucher.StartAt,
                EndAt = voucher.EndAt,
                Status = voucher.Status,
                IsActive = voucher.IsActive,
                CreatedAt = voucher.CreatedAt
            };
        }

    }
}
