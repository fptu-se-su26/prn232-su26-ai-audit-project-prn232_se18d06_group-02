using AutoMapper;
using AutoMapper.QueryableExtensions;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Admin
{
    public class AdminVoucherService : IAdminVoucherService
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public AdminVoucherService(IVoucherRepository voucherRepository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _voucherRepository = voucherRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CreateVoucherAsync(CreateVoucherDto dto)
        {
            try
            {
                var voucher = _mapper.Map<Voucher>(dto);
                
                // Auto-determine status based on dates
                voucher.Status = DetermineStatus(voucher.StartAt, voucher.EndAt);
                
                voucher.CreatedAt = DateTime.Now;

                await _voucherRepository.AddAsync(voucher);
                await _unitOfWork.SaveChangesAsync();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<PagedResult<AdminVoucherDto>> GetPaginatedVouchersAsync(AdminVoucherQueryDto query)
        {
            var pagedVouchers = await _voucherRepository.GetPaginatedAdminVouchersAsync(query);
            
            var dtos = _mapper.Map<List<AdminVoucherDto>>(pagedVouchers.Items);

            return new PagedResult<AdminVoucherDto>(dtos, pagedVouchers.TotalCount, query.PageNumber, query.PageSize);
        }

        public async Task<AdminVoucherSummaryDto> GetVoucherSummaryAsync()
        {
            return await _voucherRepository.GetAdminVoucherSummaryAsync();
        }

        public async Task<AdminVoucherDto?> GetVoucherByIdAsync(Guid id)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id);
            return _mapper.Map<AdminVoucherDto>(voucher);
        }

        public async Task<bool> UpdateVoucherAsync(Guid id, UpdateVoucherDto dto)
        {
            try
            {
                var voucher = await _voucherRepository.GetByIdAsync(id);
                if (voucher == null) return false;

                _mapper.Map(dto, voucher);
                voucher.Status = DetermineStatus(voucher.StartAt, voucher.EndAt);

                await _voucherRepository.UpdateAsync(voucher);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> ToggleVoucherStatusAsync(Guid id)
        {
            try
            {
                var voucher = await _voucherRepository.GetByIdAsync(id);
                if (voucher == null) return false;

                if (voucher.Status == VoucherStatus.Disabled)
                {
                    // Re-enable: determined status based on dates
                    voucher.Status = DetermineStatus(voucher.StartAt, voucher.EndAt);
                    voucher.IsActive = true;
                }
                else
                {
                    // Manually disable
                    voucher.Status = VoucherStatus.Disabled;
                    voucher.IsActive = false;
                }

                await _voucherRepository.UpdateAsync(voucher);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private VoucherStatus DetermineStatus(DateTime startAt, DateTime endAt)
        {
            var now = DateTime.Now;

            if (now < startAt)
            {
                return VoucherStatus.Upcoming;
            }
            
            if (now >= startAt && now <= endAt)
            {
                return VoucherStatus.Active;
            }

            return VoucherStatus.Expired;
        }
    }
}
