using AutoMapper;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Features.Admin.Mappings;

public class StoreApplicationProfile : Profile
{
    public StoreApplicationProfile()
    {
        CreateMap<Store, StoreApplicationDto>()
            .ForMember(dest => dest.BusinessType, opt => opt.MapFrom(src => src.BusinessType.ToString()))
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.OwnerUser != null ? src.OwnerUser.FullName : "Unknown"))
            .ForMember(dest => dest.OwnerEmail, opt => opt.MapFrom(src => src.OwnerUser != null ? (src.OwnerUser.Email ?? "") : ""))
            .ForMember(dest => dest.OwnerPhone, opt => opt.MapFrom(src => src.OwnerUser != null ? (src.OwnerUser.PhoneNumber ?? "") : ""))
            .ForMember(dest => dest.IdentityNumber, opt => opt.MapFrom(src => src.OwnerUser != null ? src.OwnerUser.IdentityNumber : null))
            .ForMember(dest => dest.IdentityIssuedDate, opt => opt.MapFrom(src => src.OwnerUser != null ? src.OwnerUser.IdentityIssuedDate : null))
            .ForMember(dest => dest.IdentityIssuedPlace, opt => opt.MapFrom(src => src.OwnerUser != null ? src.OwnerUser.IdentityIssuedPlace : null))
            .ForMember(dest => dest.IdentityCardFrontImageUrl, opt => opt.MapFrom(src => src.IdentityCardFrontImageUrl))
            .ForMember(dest => dest.IdentityCardBackImageUrl, opt => opt.MapFrom(src => src.IdentityCardBackImageUrl))
            .ForMember(dest => dest.ApprovedAt, opt => opt.MapFrom(src => src.ApprovedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.RejectReason, opt => opt.MapFrom(src => src.RejectReason));
    }
}
