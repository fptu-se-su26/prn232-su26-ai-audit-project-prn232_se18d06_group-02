using AutoMapper;
using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Application.Features.Admin;

public class AdminStoreService : IAdminStoreService
{
    private const int MaxStatusReasonLength = 500;

    private readonly IStoreRepository _storeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminStoreService(
        IStoreRepository storeRepository, 
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        IEmailService emailService,
        UserManager<ApplicationUser> userManager)
    {
        _storeRepository = storeRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailService = emailService;
        _userManager = userManager;
    }

    public async Task<IEnumerable<Domain.Entities.Store>> GetPendingStoresAsync()
    {
        var query = new StoreApplicationQueryDto 
        { 
            Status = StoreStatus.Pending,
            PageSize = 100 // Get all pending for the simple list
        };
        var result = await _storeRepository.GetStoreApplicationsAsync(query);
        return result.Items;
    }

    public async Task<PagedResult<StoreApplicationDto>> GetStoreApplicationsAsync(StoreApplicationQueryDto query)
    {
        var pagedStores = await _storeRepository.GetStoreApplicationsAsync(query);

        return new PagedResult<StoreApplicationDto>
        {
            Items = _mapper.Map<List<StoreApplicationDto>>(pagedStores.Items),
            TotalCount = pagedStores.TotalCount,
            PageNumber = pagedStores.PageNumber,
            PageSize = pagedStores.PageSize
        };
    }

    public async Task<StoreApplicationDto?> GetStoreApplicationByIdAsync(Guid storeId)
    {
        var store = await _storeRepository.GetStoreApplicationByIdAsync(storeId);

        if (store == null)
            return null;

        return _mapper.Map<StoreApplicationDto>(store);
    }
    public async Task<bool> UpdateStoreStatusAsync(Guid storeId, StoreStatus status, string? reason = null)
    {
        var isSendEmail = ResolveIsSendEmail(status);

        var store = await _storeRepository.GetStoreApplicationByIdAsync(storeId);
        if (store == null)
            return false;

        var normalizedReason = NormalizeStatusReason(reason);
        if (!IsValidReasonForStatus(status, normalizedReason))
            return false;

        store.Status = status;
        store.UpdatedAt = DateTime.UtcNow;

        if (status != StoreStatus.Rejected)
            store.RejectReason = null;

        if (status != StoreStatus.Locked)
            store.LockReason = null;

        if (status == StoreStatus.Approved)
        {
            store.ApprovedAt ??= DateTime.UtcNow;

            var user = store.OwnerUser;
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (!roles.Contains("Store Owner"))
                    await _userManager.AddToRoleAsync(user, "Store Owner");

                if (roles.Contains("Customer"))
                    await _userManager.RemoveFromRoleAsync(user, "Customer");
            }
        }
        else if (status == StoreStatus.Rejected)
        {
            store.RejectReason = normalizedReason;
        }
        else if (status == StoreStatus.Locked)
        {
            store.LockReason = normalizedReason;
        }

        await _storeRepository.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();

        if (isSendEmail)
        {
            await _emailService.SendStoreStatusEmailAsync(store, status, normalizedReason);
        }

        return true;
    }

    private bool ResolveIsSendEmail(StoreStatus status)
    {
        return status switch
        {
            StoreStatus.Approved => true,
            StoreStatus.Rejected => true,
            StoreStatus.Locked => true,
            _ => false
        };
    }

    private static string? NormalizeStatusReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return null;

        var trimmed = reason.Trim();
        return trimmed.Length <= MaxStatusReasonLength
            ? trimmed
            : trimmed[..MaxStatusReasonLength];
    }

    private static bool IsValidReasonForStatus(StoreStatus status, string? reason)
    {
        return status switch
        {
            StoreStatus.Rejected => !string.IsNullOrWhiteSpace(reason),
            StoreStatus.Locked => !string.IsNullOrWhiteSpace(reason),
            _ => true
        };
    }

    public async Task<bool> RequestInfoAsync(Guid storeId, string note)
    {
        var store = await _storeRepository.GetStoreApplicationByIdAsync(storeId);
        if (store == null || store.Status != StoreStatus.Pending)
            return false;

        store.UpdatedAt = DateTime.UtcNow;

        _storeRepository.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();

        if (store.OwnerUser != null && !string.IsNullOrWhiteSpace(store.OwnerUser.Email))
        {
            var subject = "Action Required: Additional Information Needed for Your Store Application";
            var body = $@"
                <h3>Dear {store.OwnerUser.FullName},</h3>
                <p>We are reviewing your application to become a seller on GearZone with your store <strong>{store.StoreName}</strong>.</p>
                <p>To proceed with your application, we need some additional information or clarification:</p>
                <p><strong>Note from Admin:</strong><br/>{note}</p>
                <br/>
                <p>Please reply to this email with the requested information at your earliest convenience.</p>
                <br/>
                <p>Best regards,<br/>The GearZone Team</p>
            ";
            await _emailService.SendAsync(store.OwnerUser.Email, subject, body);
        }

        return true;
    }

    public async Task<StoreApplicationStatsDto> GetStoreApplicationStatsAsync()
    {
        return await _storeRepository.GetStoreApplicationStatsAsync();
    }

    public async Task<List<StoreApplicationDto>> GetAllStoresAsync()
    {
        var stores = await _storeRepository.Query().Where(s => s.Status == StoreStatus.Approved).ToListAsync();
        return _mapper.Map<List<StoreApplicationDto>>(stores);
    }
}
