using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Checkout
{
    public class VoucherService : IVoucherService
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IVoucherUsageRepository _voucherUsageRepository;

        public VoucherService(
            IVoucherRepository voucherRepository,
            IVoucherUsageRepository voucherUsageRepository)
        {
            _voucherRepository = voucherRepository;
            _voucherUsageRepository = voucherUsageRepository;
        }

        public async Task<VoucherValidationResult> ValidateVoucherAsync(
            string code, string userId, decimal merchandiseTotal, decimal shippingFee, VoucherType expectedType)
        {
            var voucher = await _voucherRepository.GetByCodeAsync(code);

            if (voucher == null)
                return VoucherValidationResult.Fail("Voucher code not found.");

            if (voucher.Type != expectedType)
                return VoucherValidationResult.Fail(
                    expectedType == VoucherType.OrderDiscount
                        ? "This voucher is not an order discount voucher."
                        : "This voucher is not a shipping discount voucher.");

            if (!voucher.IsActive || voucher.Status != VoucherStatus.Active)
                return VoucherValidationResult.Fail("This voucher is no longer active.");

            var now = DateTime.Now;
            if (now < voucher.StartAt)
                return VoucherValidationResult.Fail("This voucher is not yet available.");

            if (now > voucher.EndAt)
                return VoucherValidationResult.Fail("This voucher has expired.");

            if (voucher.UsedCount >= voucher.UsageLimit)
                return VoucherValidationResult.Fail("This voucher has reached its usage limit.");

            // Check per-user usage limit
            var userUsageCount = await _voucherUsageRepository.GetUsageCountByUserAsync(voucher.Id, userId);
            if (userUsageCount >= voucher.MaxUsagePerUser)
                return VoucherValidationResult.Fail("You have already used this voucher.");

            // Check minimum order amount (always against merchandise total)
            if (voucher.MinOrderAmount.HasValue && merchandiseTotal < voucher.MinOrderAmount.Value)
                return VoucherValidationResult.Fail(
                    $"Minimum order amount is {voucher.MinOrderAmount.Value:N0}₫ to use this voucher.");

            // Calculate discount
            // For OrderDiscount: base is merchandiseTotal
            // For ShippingDiscount: base is shippingFee
            var calculationBase = voucher.Type == VoucherType.ShippingDiscount ? shippingFee : merchandiseTotal;
            var discountAmount = CalculateDiscount(voucher, calculationBase);
            
            var discountLabel = voucher.DiscountType == DiscountType.Percent
                ? $"{voucher.DiscountValue}%"
                : $"{voucher.DiscountValue:N0}₫";

            return new VoucherValidationResult
            {
                IsValid = true,
                VoucherId = voucher.Id,
                VoucherName = voucher.Name,
                VoucherCode = voucher.Code,
                DiscountAmount = discountAmount,
                DiscountLabel = discountLabel,
                Type = voucher.Type
            };
        }

        public async Task<List<AvailableVoucherDto>> GetAvailableVouchersForCheckoutAsync(
            string userId, decimal merchandiseTotal, decimal shippingFee, VoucherType type)
        {
            var vouchers = await _voucherRepository.GetAvailableVouchersAsync(type);
            var result = new List<AvailableVoucherDto>();

            foreach (var v in vouchers)
            {
                var dto = new AvailableVoucherDto
                {
                    Id = v.Id,
                    Code = v.Code,
                    Name = v.Name,
                    Description = v.Description,
                    Type = v.Type,
                    DiscountType = v.DiscountType,
                    DiscountValue = v.DiscountValue,
                    MaxDiscount = v.MaxDiscount,
                    MinOrderAmount = v.MinOrderAmount,
                    EndAt = v.EndAt,
                    UsedCount = v.UsedCount,
                    UsageLimit = v.UsageLimit,
                    IsEligible = true
                };

                // Check per-user usage
                var userUsageCount = await _voucherUsageRepository.GetUsageCountByUserAsync(v.Id, userId);
                if (userUsageCount >= v.MaxUsagePerUser)
                {
                    dto.IsEligible = false;
                    dto.IneligibleReason = "Limit per user reached";
                }
                else if (v.MinOrderAmount.HasValue && merchandiseTotal < v.MinOrderAmount.Value)
                {
                    dto.IsEligible = false;
                    dto.IneligibleReason = $"Min. order {v.MinOrderAmount.Value:N0}₫";
                }

                result.Add(dto);
            }

            // Eligible ones first, then ineligible
            return result.OrderByDescending(v => v.IsEligible)
                         .ThenByDescending(v => v.DiscountValue)
                         .ToList();
        }

        public async Task RecordVoucherUsageAsync(Guid voucherId, string userId, Guid orderId, decimal discountAmount)
        {
            var voucher = await _voucherRepository.GetByIdAsync(voucherId);
            if (voucher != null)
            {
                voucher.UsedCount++;
                await _voucherRepository.UpdateAsync(voucher);
            }

            var usage = new VoucherUsage
            {
                Id = Guid.NewGuid(),
                VoucherId = voucherId,
                UserId = userId,
                OrderId = orderId,
                DiscountAmount = discountAmount,
                UsedAt = DateTime.Now
            };

            await _voucherUsageRepository.AddAsync(usage);
        }

        private static decimal CalculateDiscount(Voucher voucher, decimal calculationBase)
        {
            decimal discount;
            if (voucher.DiscountType == DiscountType.Percent)
            {
                discount = calculationBase * voucher.DiscountValue / 100;
                if (voucher.MaxDiscount.HasValue && discount > voucher.MaxDiscount.Value)
                    discount = voucher.MaxDiscount.Value;
            }
            else // FixedAmount
            {
                discount = voucher.DiscountValue;
            }

            // Discount can't exceed the base (e.g. shipping discount can't exceed shipping fee)
            return Math.Min(discount, calculationBase);
        }
    }
}
