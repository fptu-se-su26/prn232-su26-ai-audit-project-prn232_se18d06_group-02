using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Entities;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services;

public interface ISellerStoreService
{
    Task<bool> ApplyForStoreAsync(StoreRegistrationDto storeRegistrationDto);
    Task<Store?> GetStoreByOwnerIdAsync(string userId);

    // Multi-step Registration
    Task<Guid> SaveStep1Async(string userId, Step1Dto dto);
    Task SaveStep2Async(Guid storeId, string userId, Step2Dto dto);
    Task SaveStep3Async(Guid storeId, Step3Dto dto);
    Task SubmitRegistrationAsync(Guid storeId, string userId);
    Task<RegistrationProgressDto?> GetRegistrationProgressAsync(string userId);
    Task<Guid> StartReapplicationAsync(string userId);

    // Profile Management
    Task<bool> UpdateStoreProfileAsync(string userId, UpdateStoreProfileDto dto);
}
