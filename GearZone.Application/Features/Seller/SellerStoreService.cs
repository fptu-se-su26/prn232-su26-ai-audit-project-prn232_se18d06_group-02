using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Abstractions.External;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Seller;

public class SellerStoreService : ISellerStoreService
{
    private static readonly HashSet<string> AllowedIdentityImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    };
    private static readonly HashSet<string> AllowedIdentityImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };
    private const long MaxIdentityImageSizeInBytes = 5 * 1024 * 1024; // 5MB

    private readonly IStoreRepository _storeRepository;
    private readonly ISystemSettingRepository _systemSettingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBankCatalogService _bankCatalogService;
    private readonly IFileStorageService _fileStorageService;

    public SellerStoreService(
        IStoreRepository storeRepository,
        ISystemSettingRepository systemSettingRepository,
        IUnitOfWork unitOfWork,
        IBankCatalogService bankCatalogService,
        IFileStorageService fileStorageService)
    {
        _storeRepository = storeRepository;
        _systemSettingRepository = systemSettingRepository;
        _unitOfWork = unitOfWork;
        _bankCatalogService = bankCatalogService;
        _fileStorageService = fileStorageService;
    }

    public async Task<bool> ApplyForStoreAsync(StoreRegistrationDto storeRegistrationDto)
    {
        var setting = await _systemSettingRepository.GetByKeyAsync("Payment_CommissionRate");
        decimal commissionRate = 0.05m;
        if (setting != null && decimal.TryParse(setting.Value, out decimal parsedRate))
        {
            commissionRate = parsedRate;
        }

        var store = new Store
        {
            Id = Guid.NewGuid(),
            OwnerUserId = storeRegistrationDto.OwnerUserId,
            StoreName = storeRegistrationDto.StoreName,
            Slug = storeRegistrationDto.StoreName.ToLower().Replace(" ", "-"),
            TaxCode = storeRegistrationDto.TaxCode,
            Phone = storeRegistrationDto.Phone,
            Email = storeRegistrationDto.Email,
            AddressLine = storeRegistrationDto.AddressLine,
            Province = storeRegistrationDto.Province,
            Status = StoreStatus.Pending,
            CommissionRate = commissionRate,
            CreatedAt = DateTime.UtcNow
        };

        await _storeRepository.AddAsync(store);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<Store?> GetStoreByOwnerIdAsync(string userId)
    {
        return await _storeRepository.GetStoreByOwnerIdAsync(userId);
    }

    public async Task<bool> UpdateStoreProfileAsync(string userId, UpdateStoreProfileDto dto)
    {
        var store = await _storeRepository.Query()
            .FirstOrDefaultAsync(s => s.OwnerUserId == userId && s.Status == StoreStatus.Approved);

        if (store == null) return false;

        store.Phone = dto.Phone;
        store.Email = dto.Email;
        store.Description = dto.Description;
        store.AddressLine = dto.AddressLine;
        store.Province = dto.Province;
        store.Latitude = dto.Latitude;
        store.Longitude = dto.Longitude;
        store.UpdatedAt = DateTime.UtcNow;

        _storeRepository.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // ==========================================
    // Multi-step Registration
    // ==========================================

    public async Task<Guid> SaveStep1Async(string userId, Step1Dto dto)
    {
        // Check if a draft already exists for this user
        var existing = await _storeRepository.Query()
            .FirstOrDefaultAsync(s => s.OwnerUserId == userId && s.Status == StoreStatus.Draft);

        if (existing != null)
        {
            existing.StoreName = dto.StoreName;
            existing.Slug = dto.StoreName.ToLower().Replace(" ", "-");
            existing.BusinessType = dto.BusinessType;
            existing.Phone = dto.Phone;
            existing.Email = dto.Email;
            existing.AddressLine = dto.AddressLine;
            existing.Province = dto.Province;
            existing.RegistrationStep = Math.Max(existing.RegistrationStep, 2);
            existing.UpdatedAt = DateTime.UtcNow;

            _storeRepository.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();
            return existing.Id;
        }

        var setting = await _systemSettingRepository.GetByKeyAsync("Payment_CommissionRate");
        decimal commissionRate = 0.05m;
        if (setting != null && decimal.TryParse(setting.Value, out decimal parsedRate))
        {
            commissionRate = parsedRate;
        }

        var store = new Store
        {
            Id = Guid.NewGuid(),
            OwnerUserId = userId,
            StoreName = dto.StoreName,
            Slug = dto.StoreName.ToLower().Replace(" ", "-"),
            BusinessType = dto.BusinessType,
            Phone = dto.Phone,
            Email = dto.Email,
            AddressLine = dto.AddressLine,
            Province = dto.Province,
            Status = StoreStatus.Draft,
            RegistrationStep = 2,
            CommissionRate = commissionRate,
            CreatedAt = DateTime.UtcNow
        };

        await _storeRepository.AddAsync(store);
        await _unitOfWork.SaveChangesAsync();
        return store.Id;
    }

    public async Task SaveStep2Async(Guid storeId, string userId, Step2Dto dto)
    {
        var store = await _storeRepository.Query()
            .Include(s => s.OwnerUser)
            .FirstOrDefaultAsync(s => s.Id == storeId && s.OwnerUserId == userId && s.Status == StoreStatus.Draft);

        if (store == null) throw new InvalidOperationException("Store not found or not in draft status.");

        if (dto.IdentityCardFrontImage != null)
        {
            ValidateIdentityImage(dto.IdentityCardFrontImage, "front");
        }

        if (dto.IdentityCardBackImage != null)
        {
            ValidateIdentityImage(dto.IdentityCardBackImage, "back");
        }

        if (string.IsNullOrWhiteSpace(store.IdentityCardFrontImageUrl) && dto.IdentityCardFrontImage == null)
        {
            throw new InvalidOperationException("Please upload the front ID card image.");
        }

        if (string.IsNullOrWhiteSpace(store.IdentityCardBackImageUrl) && dto.IdentityCardBackImage == null)
        {
            throw new InvalidOperationException("Please upload the back ID card image.");
        }

        string? newFrontImageUrl = null;
        string? newBackImageUrl = null;
        var justUploadedUrls = new List<string>();

        try
        {
            if (dto.IdentityCardFrontImage != null)
            {
                newFrontImageUrl = await UploadIdentityImageAsync(storeId, userId, dto.IdentityCardFrontImage, "front");
                justUploadedUrls.Add(newFrontImageUrl);
            }

            if (dto.IdentityCardBackImage != null)
            {
                newBackImageUrl = await UploadIdentityImageAsync(storeId, userId, dto.IdentityCardBackImage, "back");
                justUploadedUrls.Add(newBackImageUrl);
            }
        }
        catch
        {
            foreach (var uploadedUrl in justUploadedUrls)
            {
                await _fileStorageService.DeleteAsync(uploadedUrl);
            }

            throw;
        }

        if (!string.IsNullOrWhiteSpace(newFrontImageUrl))
        {
            if (!string.IsNullOrWhiteSpace(store.IdentityCardFrontImageUrl))
            {
                await _fileStorageService.DeleteAsync(store.IdentityCardFrontImageUrl);
            }

            store.IdentityCardFrontImageUrl = newFrontImageUrl;
        }

        if (!string.IsNullOrWhiteSpace(newBackImageUrl))
        {
            if (!string.IsNullOrWhiteSpace(store.IdentityCardBackImageUrl))
            {
                await _fileStorageService.DeleteAsync(store.IdentityCardBackImageUrl);
            }

            store.IdentityCardBackImageUrl = newBackImageUrl;
        }

        store.TaxCode = dto.TaxCode;
        store.RegistrationStep = Math.Max(store.RegistrationStep, 3);
        store.UpdatedAt = DateTime.UtcNow;

        // Update user identity info
        if (store.OwnerUser != null)
        {
            store.OwnerUser.FullName = dto.FullName;
            store.OwnerUser.IdentityNumber = dto.IdentityNumber;
            store.OwnerUser.IdentityIssuedDate = dto.IdentityIssuedDate;
            store.OwnerUser.IdentityIssuedPlace = dto.IdentityIssuedPlace;
        }

        _storeRepository.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SaveStep3Async(Guid storeId, Step3Dto dto)
    {
        var store = await _storeRepository.Query()
            .FirstOrDefaultAsync(s => s.Id == storeId && s.Status == StoreStatus.Draft);

        if (store == null) throw new InvalidOperationException("Store not found or not in draft status.");
        var selectedBank = _bankCatalogService.FindByName(dto.BankName)
            ?? throw new InvalidOperationException("Selected bank is not supported.");

        store.BankName = selectedBank.Name;
        store.BankAccountNumber = dto.BankAccountNumber;
        store.BankAccountName = dto.BankAccountName;
        store.BankBin = selectedBank.Bin;
        store.RegistrationStep = Math.Max(store.RegistrationStep, 4);
        store.UpdatedAt = DateTime.UtcNow;

        _storeRepository.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SubmitRegistrationAsync(Guid storeId, string userId)
    {
        var store = await _storeRepository.Query()
            .FirstOrDefaultAsync(s => s.Id == storeId && s.OwnerUserId == userId && s.Status == StoreStatus.Draft);

        if (store == null) throw new InvalidOperationException("Store not found or not in draft status.");
        if (string.IsNullOrWhiteSpace(store.IdentityCardFrontImageUrl) || string.IsNullOrWhiteSpace(store.IdentityCardBackImageUrl))
        {
            throw new InvalidOperationException("Please upload both front and back ID card images before submitting.");
        }

        store.Status = StoreStatus.Pending;
        store.UpdatedAt = DateTime.UtcNow;

        _storeRepository.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<RegistrationProgressDto?> GetRegistrationProgressAsync(string userId)
    {
        var store = await _storeRepository.Query()
            .Include(s => s.OwnerUser)
            .FirstOrDefaultAsync(s => s.OwnerUserId == userId && s.Status == StoreStatus.Draft);

        if (store == null) return null;

        return new RegistrationProgressDto
        {
            StoreId = store.Id,
            CurrentStep = store.RegistrationStep,
            Step1 = new Step1Dto
            {
                StoreName = store.StoreName,
                BusinessType = store.BusinessType,
                Phone = store.Phone,
                Email = store.Email,
                AddressLine = store.AddressLine,
                Province = store.Province
            },
            Step2 = new Step2Dto
            {
                FullName = store.OwnerUser?.FullName ?? string.Empty,
                IdentityNumber = store.OwnerUser?.IdentityNumber ?? string.Empty,
                IdentityIssuedDate = store.OwnerUser?.IdentityIssuedDate,
                IdentityIssuedPlace = store.OwnerUser?.IdentityIssuedPlace ?? string.Empty,
                TaxCode = store.TaxCode,
                IdentityCardFrontImageUrl = store.IdentityCardFrontImageUrl,
                IdentityCardBackImageUrl = store.IdentityCardBackImageUrl
            },
            Step3 = new Step3Dto
            {
                BankName = store.BankName,
                BankAccountNumber = store.BankAccountNumber,
                BankAccountName = store.BankAccountName,
                BankBin = store.BankBin
            }
        };
    }

    public async Task<Guid> StartReapplicationAsync(string userId)
    {
        var existingDraft = await _storeRepository.Query()
            .Where(s => s.OwnerUserId == userId && s.Status == StoreStatus.Draft)
            .OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt)
            .FirstOrDefaultAsync();

        if (existingDraft != null)
        {
            return existingDraft.Id;
        }

        var revisableStore = await _storeRepository.Query()
            .Where(s => s.OwnerUserId == userId
                && (s.Status == StoreStatus.Rejected || s.Status == StoreStatus.Pending))
            .OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt)
            .FirstOrDefaultAsync();

        if (revisableStore == null)
        {
            throw new InvalidOperationException("No pending or rejected application found to re-apply.");
        }

        revisableStore.Status = StoreStatus.Draft;
        revisableStore.RejectReason = null;
        revisableStore.RegistrationStep = 1;
        revisableStore.ApprovedAt = null;
        revisableStore.UpdatedAt = DateTime.UtcNow;

        await _storeRepository.UpdateAsync(revisableStore);
        await _unitOfWork.SaveChangesAsync();

        return revisableStore.Id;
    }

    private async Task<string> UploadIdentityImageAsync(Guid storeId, string userId, IFormFile file, string side)
    {
        var folder = $"GearZone/kyc/{storeId}/{userId}/{side}";
        var uploadedUrls = await _fileStorageService.UploadAsync(new List<IFormFile> { file }, folder);
        var imageUrl = uploadedUrls.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new InvalidOperationException($"Failed to upload {side} ID card image.");
        }

        return imageUrl;
    }

    private static void ValidateIdentityImage(IFormFile file, string side)
    {
        if (file.Length <= 0)
        {
            throw new InvalidOperationException($"The {side} ID card image is empty.");
        }

        if (file.Length > MaxIdentityImageSizeInBytes)
        {
            throw new InvalidOperationException($"The {side} ID card image exceeds the 5MB limit.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedIdentityImageExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"The {side} ID card image must be JPG, PNG, or WEBP.");
        }

        if (string.IsNullOrWhiteSpace(file.ContentType) || !AllowedIdentityImageContentTypes.Contains(file.ContentType))
        {
            throw new InvalidOperationException($"The {side} ID card image content type is not supported.");
        }
    }
}
