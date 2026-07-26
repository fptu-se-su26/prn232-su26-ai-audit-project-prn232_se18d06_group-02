using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Application.Features.Checkout
{
    public class VoucherService : IVoucherService
    {
        private readonly IVoucherRepository _vouchers;
        private readonly IVoucherUsageRepository _usages;
        private readonly ICategoryRepository _categories;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public VoucherService(
            IVoucherRepository vouchers,
            IVoucherUsageRepository usages,
            ICategoryRepository categories,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            _vouchers = vouchers;
            _usages = usages;
            _categories = categories;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public Task<VoucherValidationResult> ValidateVoucherAsync(
            string code,
            string userId,
            decimal merchandiseTotal,
            decimal shippingFee,
            VoucherType expectedType)
        {
            var context = new VoucherEvaluationContextDto
            {
                Lines =
                {
                    new VoucherEvaluationLineDto
                    {
                        StoreId = Guid.Empty,
                        CategoryId = 0,
                        EffectiveSubtotal = merchandiseTotal
                    }
                },
                ShippingFees =
                {
                    new VoucherShippingFeeDto
                    {
                        StoreId = Guid.Empty,
                        ShippingFee = shippingFee
                    }
                }
            };
            return ValidateVoucherForContextAsync(code, userId, context, expectedType);
        }

        public Task<List<AvailableVoucherDto>> GetAvailableVouchersForCheckoutAsync(
            string userId,
            decimal merchandiseTotal,
            decimal shippingFee,
            VoucherType type)
        {
            var context = new VoucherEvaluationContextDto
            {
                Lines =
                {
                    new VoucherEvaluationLineDto
                    {
                        StoreId = Guid.Empty,
                        CategoryId = 0,
                        EffectiveSubtotal = merchandiseTotal
                    }
                },
                ShippingFees =
                {
                    new VoucherShippingFeeDto
                    {
                        StoreId = Guid.Empty,
                        ShippingFee = shippingFee
                    }
                }
            };
            return GetAvailableVouchersForContextAsync(userId, context, type);
        }

        public async Task<VoucherValidationResult> ValidateVoucherForContextAsync(
            string code,
            string userId,
            VoucherEvaluationContextDto context,
            VoucherType expectedType,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return VoucherValidationResult.Fail("Please enter a voucher code.");
            }

            var voucher = await _vouchers.GetByCodeAsync(code);
            if (voucher == null)
            {
                return VoucherValidationResult.Fail("Voucher code not found.");
            }

            return await EvaluateAsync(voucher, userId, context, expectedType, ct);
        }

        public async Task<List<AvailableVoucherDto>> GetAvailableVouchersForContextAsync(
            string userId,
            VoucherEvaluationContextDto context,
            VoucherType type,
            CancellationToken ct = default)
        {
            var vouchers = await _vouchers.GetAvailableVouchersAsync(type);
            var result = new List<AvailableVoucherDto>();

            foreach (var voucher in vouchers)
            {
                var validation = await EvaluateAsync(voucher, userId, context, type, ct);
                result.Add(new AvailableVoucherDto
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
                    EndAt = voucher.EndAt,
                    UsedCount = voucher.UsedCount,
                    UsageLimit = voucher.UsageLimit,
                    Scope = voucher.Scope,
                    StoreId = voucher.StoreId,
                    StoreName = voucher.Store?.StoreName,
                    IsEligible = validation.IsValid,
                    IneligibleReason = validation.ErrorMessage
                });
            }

            return result
                .OrderByDescending(x => x.IsEligible)
                .ThenBy(x => x.EndAt)
                .ThenByDescending(x => x.DiscountValue)
                .ToList();
        }

        public async Task<bool> ReserveVoucherUsageAsync(
            Guid voucherId,
            string userId,
            Guid orderId,
            decimal discountAmount,
            CancellationToken ct = default)
        {
            var voucher = await _vouchers.GetByIdAsync(voucherId, ct);
            if (voucher == null)
            {
                return false;
            }

            var userUsageCount = await _usages.GetUsageCountByUserAsync(voucherId, userId);
            if (userUsageCount >= voucher.MaxUsagePerUser)
            {
                return false;
            }

            if (!await _vouchers.TryReserveUsageAsync(
                    voucherId,
                    _timeProvider.GetUtcNow().UtcDateTime,
                    ct))
            {
                return false;
            }

            await _usages.AddAsync(new VoucherUsage
            {
                Id = Guid.NewGuid(),
                VoucherId = voucherId,
                UserId = userId,
                OrderId = orderId,
                DiscountAmount = discountAmount,
                UsedAt = _timeProvider.GetUtcNow().UtcDateTime,
                Status = VoucherUsageStatus.Reserved
            }, ct);

            return true;
        }

        public async Task RedeemOrderVouchersAsync(
            Guid orderId,
            Guid? storeId = null,
            CancellationToken ct = default,
            bool includePlatform = true)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var usages = await _usages.GetByOrderAsync(orderId, ct);
            foreach (var usage in usages.Where(x =>
                         x.Status == VoucherUsageStatus.Reserved &&
                         (!storeId.HasValue ||
                          (includePlatform &&
                           x.Voucher.Scope == VoucherScope.Platform) ||
                          x.Voucher.StoreId == storeId.Value)))
            {
                usage.Status = VoucherUsageStatus.Redeemed;
                usage.RedeemedAt = now;
                await _usages.UpdateAsync(usage);
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task ReleaseOrderVouchersAsync(
            Guid orderId,
            Guid? storeId = null,
            CancellationToken ct = default,
            bool includePlatform = true)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var usages = await _usages.GetByOrderAsync(orderId, ct);
            foreach (var usage in usages.Where(x =>
                         x.Status != VoucherUsageStatus.Released &&
                         (!storeId.HasValue ||
                          (includePlatform &&
                           x.Voucher.Scope == VoucherScope.Platform) ||
                          x.Voucher.StoreId == storeId.Value)))
            {
                usage.Status = VoucherUsageStatus.Released;
                usage.ReleasedAt = now;
                await _usages.UpdateAsync(usage);
                await _vouchers.ReleaseUsageCapacityAsync(usage.VoucherId, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task RecordVoucherUsageAsync(
            Guid voucherId,
            string userId,
            Guid orderId,
            decimal discountAmount)
        {
            if (await ReserveVoucherUsageAsync(
                    voucherId,
                    userId,
                    orderId,
                    discountAmount))
            {
                await _unitOfWork.SaveChangesAsync();
            }
        }

        private async Task<VoucherValidationResult> EvaluateAsync(
            Voucher voucher,
            string userId,
            VoucherEvaluationContextDto context,
            VoucherType expectedType,
            CancellationToken ct)
        {
            if (voucher.Type != expectedType)
            {
                return VoucherValidationResult.Fail(
                    expectedType == VoucherType.OrderDiscount
                        ? "This voucher is not an order discount voucher."
                        : "This voucher is not a shipping discount voucher.");
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            if (!voucher.IsActive)
            {
                return VoucherValidationResult.Fail("This voucher is disabled.");
            }

            if (now < voucher.StartAt)
            {
                return VoucherValidationResult.Fail("This voucher is not yet available.");
            }

            if (now >= voucher.EndAt)
            {
                return VoucherValidationResult.Fail("This voucher has expired.");
            }

            if (voucher.UsedCount >= voucher.UsageLimit)
            {
                return VoucherValidationResult.Fail("This voucher has reached its usage limit.");
            }

            var userUsageCount = await _usages.GetUsageCountByUserAsync(voucher.Id, userId);
            if (userUsageCount >= voucher.MaxUsagePerUser)
            {
                return VoucherValidationResult.Fail("Limit per user reached.");
            }

            var scopedLines = voucher.Scope == VoucherScope.Seller
                ? context.Lines.Where(x => x.StoreId == voucher.StoreId).ToList()
                : context.Lines.ToList();

            if (voucher.CategoryId.HasValue && voucher.Type == VoucherType.OrderDiscount)
            {
                var categoryParents = await _categories.Query()
                    .AsNoTracking()
                    .Select(x => new { x.Id, x.ParentId })
                    .ToDictionaryAsync(x => x.Id, x => x.ParentId, ct);

                scopedLines = scopedLines
                    .Where(x => IsCategoryOrDescendant(
                        x.CategoryId,
                        voucher.CategoryId.Value,
                        categoryParents))
                    .ToList();
            }

            var eligibleMerchandise = scopedLines.Sum(x => x.EffectiveSubtotal);
            if (eligibleMerchandise <= 0)
            {
                return VoucherValidationResult.Fail(
                    voucher.Scope == VoucherScope.Seller
                        ? "This voucher does not apply to the selected store items."
                        : "No selected items are eligible for this voucher.");
            }

            if (voucher.MinOrderAmount.HasValue &&
                eligibleMerchandise < voucher.MinOrderAmount.Value)
            {
                return VoucherValidationResult.Fail(
                    $"Minimum eligible spend is {voucher.MinOrderAmount.Value:N0}₫.");
            }

            var calculationBase = eligibleMerchandise;
            if (voucher.Type == VoucherType.ShippingDiscount)
            {
                calculationBase = voucher.Scope == VoucherScope.Seller
                    ? context.ShippingFees
                        .Where(x => x.StoreId == voucher.StoreId)
                        .Sum(x => x.ShippingFee)
                    : context.ShippingTotal;

                if (calculationBase <= 0)
                {
                    return VoucherValidationResult.Fail(
                        "No shipping fee is eligible for this voucher.");
                }
            }

            var discountAmount = CalculateDiscount(voucher, calculationBase);
            return new VoucherValidationResult
            {
                IsValid = true,
                VoucherId = voucher.Id,
                VoucherName = voucher.Name,
                VoucherCode = voucher.Code,
                DiscountAmount = discountAmount,
                DiscountLabel = voucher.DiscountType == DiscountType.Percent
                    ? $"{voucher.DiscountValue}%"
                    : $"{voucher.DiscountValue:N0}₫",
                Type = voucher.Type,
                Scope = voucher.Scope,
                StoreId = voucher.StoreId
            };
        }

        private static bool IsCategoryOrDescendant(
            int categoryId,
            int ancestorId,
            IReadOnlyDictionary<int, int?> parents)
        {
            var current = (int?)categoryId;
            var visited = new HashSet<int>();
            while (current.HasValue && visited.Add(current.Value))
            {
                if (current.Value == ancestorId)
                {
                    return true;
                }

                current = parents.GetValueOrDefault(current.Value);
            }

            return false;
        }

        private static decimal CalculateDiscount(Voucher voucher, decimal calculationBase)
        {
            var discount = voucher.DiscountType == DiscountType.Percent
                ? calculationBase * voucher.DiscountValue / 100m
                : voucher.DiscountValue;

            if (voucher.MaxDiscount.HasValue)
            {
                discount = Math.Min(discount, voucher.MaxDiscount.Value);
            }

            return Math.Round(
                Math.Min(Math.Max(0, discount), calculationBase),
                2,
                MidpointRounding.AwayFromZero);
        }
    }
}
