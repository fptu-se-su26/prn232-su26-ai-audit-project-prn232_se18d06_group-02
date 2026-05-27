using AutoMapper;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GearZone.Application.Abstractions.External;

namespace GearZone.Application.Features.Admin;

public class AdminBrandService : IAdminBrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;

    public AdminBrandService(IBrandRepository brandRepository, IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService)
    {
        _brandRepository = brandRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
    }

    public async Task<PagedResult<AdminBrandDto>> GetBrandsAsync(AdminBrandQueryDto query)
    {
        var pagedBrands = await _brandRepository.GetAdminBrandsAsync(query);

        return new PagedResult<AdminBrandDto>
        {
            Items = _mapper.Map<List<AdminBrandDto>>(pagedBrands.Items),
            TotalCount = pagedBrands.TotalCount,
            PageNumber = pagedBrands.PageNumber,
            PageSize = pagedBrands.PageSize
        };
    }

    public async Task<List<AdminBrandDto>> GetAllBrandsListAsync()
    {
        var brands = await _brandRepository.GetAllBrandsListAsync();
        return _mapper.Map<List<AdminBrandDto>>(brands);
    }

    public async Task<AdminBrandDto?> GetBrandByIdAsync(int brandId)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId);
        if (brand == null) return null;

        return _mapper.Map<AdminBrandDto>(brand);
    }

    public async Task<bool> ApproveBrandAsync(int brandId)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId);
        if (brand == null || brand.IsApproved) return false;

        brand.IsApproved = true;
        brand.UpdatedAt = DateTime.UtcNow;

        _brandRepository.UpdateAsync(brand);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RejectBrandAsync(int brandId)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId);
        if (brand == null || !brand.IsApproved) return false;

        brand.IsApproved = false;
        brand.UpdatedAt = DateTime.UtcNow;

        _brandRepository.UpdateAsync(brand);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteBrandAsync(int brandId)
    {
        var brand = await _brandRepository.GetByIdAsync(brandId);
        if (brand == null || brand.IsDeleted) return false;

        brand.IsDeleted = true;
        brand.UpdatedAt = DateTime.UtcNow;

        _brandRepository.UpdateAsync(brand);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<AdminBrandStatsDto> GetBrandStatsAsync()
    {
        return await _brandRepository.GetBrandStatsAsync();
    }

    public async Task<bool> CreateBrandAsync(CreateBrandDto dto)
    {
        try
        {
            if (dto.LogoFile != null)
            {
                var uploadedUrls = await _fileStorageService.UploadAsync(new List<IFormFile> { dto.LogoFile });
                if (uploadedUrls.Any())
                {
                    dto.LogoUrl = uploadedUrls.First();
                }
            }

            var brand = _mapper.Map<GearZone.Domain.Entities.Brand>(dto);
            brand.CreatedAt = DateTime.UtcNow;

            await _brandRepository.AddAsync(brand);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateBrandAsync(EditBrandDto dto)
    {
        try
        {
            var brand = await _brandRepository.GetByIdAsync(dto.Id);
            if (brand == null || brand.IsDeleted) return false;

            if (dto.LogoFile != null)
            {
                var uploadedUrls = await _fileStorageService.UploadAsync(new List<IFormFile> { dto.LogoFile });
                if (uploadedUrls.Any())
                {
                    if (!string.IsNullOrEmpty(brand.LogoUrl))
                    {
                        await _fileStorageService.DeleteAsync(brand.LogoUrl);
                    }
                    dto.LogoUrl = uploadedUrls.First();
                }
            }
            else if (dto.LogoUrl != brand.LogoUrl && !string.IsNullOrEmpty(brand.LogoUrl))
            {
                // LogoURL changed (e.g., cleared out or replaced with external URL)
                await _fileStorageService.DeleteAsync(brand.LogoUrl);
            }

            _mapper.Map(dto, brand);
            brand.UpdatedAt = DateTime.UtcNow;

            _brandRepository.UpdateAsync(brand);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
