using System.Collections.Generic;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Features.Seller.Dtos
{
    /// <summary>Everything the seller voucher listing screen needs in one payload.</summary>
    public class SellerVoucherListDto
    {
        public PagedResult<SellerVoucherDto> Vouchers { get; set; } = new();
        public SellerVoucherSummaryDto Summary { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
    }
}
