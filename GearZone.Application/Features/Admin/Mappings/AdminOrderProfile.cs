using System.Linq;
using AutoMapper;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Features.Admin.Mappings
{
    public class AdminOrderProfile : Profile
    {
        public AdminOrderProfile()
        {
            CreateMap<Order, AdminOrderDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.OrderCode, opt => opt.MapFrom(src => src.OrderCode))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
                .ForMember(dest => dest.ReceiverName, opt => opt.MapFrom(src => src.ReceiverName))
                .ForMember(dest => dest.GrandTotal, opt => opt.MapFrom(src => src.GrandTotal))
                .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.ShippingAddress))
                .ForMember(dest => dest.ReceiverPhone, opt => opt.MapFrom(src => src.ReceiverPhone))
                .ForMember(dest => dest.PaidAt, opt => opt.MapFrom(src => src.PaidAt));

            CreateMap<Order, AdminOrderDetailDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty));

            CreateMap<SubOrder, AdminSubOrderDto>()
                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store != null ? src.Store.StoreName : string.Empty))
                .ForMember(dest => dest.StoreEmail, opt => opt.MapFrom(src => src.Store != null ? src.Store.Email : string.Empty))
                .ForMember(dest => dest.CommissionRate, opt => opt.MapFrom(src => src.CommissionRateSnapshot));

            CreateMap<OrderItem, AdminOrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductNameSnapshot))
                .ForMember(dest => dest.VariantName, opt => opt.MapFrom(src => src.VariantNameSnapshot))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPriceSnapshot))
                .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.Variant != null && src.Variant.Product != null ? src.Variant.Product.Images.FirstOrDefault(i => i.IsPrimary).ImageUrl : null));

            CreateMap<OrderStatusHistory, AdminOrderStatusHistoryDto>();

            CreateMap<Domain.Entities.Payment, AdminOrderPaymentDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method.ToString()));
        }
    }
}
