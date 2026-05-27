using AutoMapper;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Features.Admin.Mappings
{
    public class AdminPayoutBatchProfile : Profile
    {
        public AdminPayoutBatchProfile()
        {
            CreateMap<PayoutBatch, AdminPayoutBatchDto>()
                .ForMember(dest => dest.Transactions, opt => opt.Ignore());

            CreateMap<PayoutTransaction, AdminPayoutTransactionDto>()
                .ForMember(dest => dest.TransactionCode, opt => opt.MapFrom(src => src.TransactionCode))
                .ForMember(dest => dest.BatchCode, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.BatchCode : string.Empty))
                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store != null ? src.Store.StoreName : string.Empty))
                .ForMember(dest => dest.StoreEmail, opt => opt.MapFrom(src => src.Store != null ? src.Store.OwnerUser.Email : string.Empty))
                .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.BankName))
                .ForMember(dest => dest.BankAccountNumber, opt => opt.MapFrom(src => src.BankAccountNumber));

            CreateMap<PayoutTransaction, AdminPayoutTransactionDetailDto>()
                .ForMember(dest => dest.BatchCode, opt => opt.MapFrom(src => src.Batch.BatchCode))
                .ForMember(dest => dest.BatchCreatedAt, opt => opt.MapFrom(src => src.Batch.CreatedAt))
                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store.StoreName))
                .ForMember(dest => dest.StoreEmail, opt => opt.MapFrom(src => src.Store.OwnerUser.Email))
                .ForMember(dest => dest.StorePhone, opt => opt.MapFrom(src => src.Store.Phone))
                .ForMember(dest => dest.StoreOwnerName, opt => opt.MapFrom(src => src.Store.OwnerUser.FullName))
                .ForMember(dest => dest.StoreCommissionRate, opt => opt.MapFrom(src => src.Store.CommissionRate))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<PayoutItem, AdminPayoutItemDto>()
                .ForMember(dest => dest.OrderCode, opt => opt.MapFrom(src => src.SubOrder.Order.OrderCode))
                .ForMember(dest => dest.OrderCreatedAt, opt => opt.MapFrom(src => src.SubOrder.CreatedAt))
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.SubOrder.Status.ToString()));
        }
    }
}
