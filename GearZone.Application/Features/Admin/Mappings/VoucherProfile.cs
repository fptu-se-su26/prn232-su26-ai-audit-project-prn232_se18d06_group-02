using AutoMapper;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Admin.Mappings
{
    public class VoucherProfile : Profile
    {
        public VoucherProfile()
        {
            CreateMap<CreateVoucherDto, Voucher>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 
                    src.Type == "Shipping" ? VoucherType.ShippingDiscount : VoucherType.OrderDiscount))
                .ForMember(dest => dest.DiscountType, opt => opt.MapFrom(src => 
                    src.DiscountType == "Percent" ? DiscountType.Percent : DiscountType.FixedAmount))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code.ToUpper().Trim()))
                .ForMember(dest => dest.Scope, opt => opt.MapFrom(_ => VoucherScope.Platform))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsVisible))
                .ForMember(dest => dest.MaxUsagePerUser, opt => opt.MapFrom(src => src.MaxUsagePerUser));

            CreateMap<UpdateVoucherDto, Voucher>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src =>
                    src.Type == "Shipping" ? VoucherType.ShippingDiscount : VoucherType.OrderDiscount))
                .ForMember(dest => dest.DiscountType, opt => opt.MapFrom(src =>
                    src.DiscountType == "Percent" ? DiscountType.Percent : DiscountType.FixedAmount))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code.ToUpper().Trim()))
                .ForMember(dest => dest.Scope, opt => opt.MapFrom(_ => VoucherScope.Platform))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsVisible))
                .ForMember(dest => dest.MaxUsagePerUser, opt => opt.MapFrom(src => src.MaxUsagePerUser));

            CreateMap<Voucher, AdminVoucherDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "General"))
                .ForMember(dest => dest.CategoryIcon, opt => opt.MapFrom(src => 
                    src.Type == VoucherType.ShippingDiscount ? "local_shipping" : 
                    (src.Category != null ? (
                        src.Category.Slug.Contains("keyboard") ? "keyboard" :
                        src.Category.Slug.Contains("mouse") || src.Category.Slug.Contains("mice") ? "mouse" :
                        src.Category.Slug.Contains("headset") || src.Category.Slug.Contains("headphone") ? "headset" :
                        src.Category.Slug.Contains("monitor") ? "monitor" :
                        src.Category.Slug.Contains("pc-components") || src.Category.Slug.Contains("cpu") || src.Category.Slug.Contains("gpu") ? "memory" :
                        src.Category.Slug.Contains("furniture") ? "chair" :
                        src.Category.Slug.Contains("console") || src.Category.Slug.Contains("controller") ? "videogame_asset" :
                        "category"
                    ) : "confirmation_number")));
        }
    }
}
