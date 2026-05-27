using AutoMapper;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Admin
{
    public class AdminPayoutService : IAdminPayoutService
    {
        private readonly IPayoutBatchRepository _batchRepo;
        private readonly IPayoutTransactionRepository _txRepo;
        private readonly ISubOrderRepository _subOrderRepo;
        private readonly IMapper _mapper;

        public AdminPayoutService(
            IPayoutBatchRepository batchRepo,
            IPayoutTransactionRepository txRepo,
            ISubOrderRepository subOrderRepo,
            IMapper mapper)
        {
            _batchRepo = batchRepo;
            _txRepo = txRepo;
            _subOrderRepo = subOrderRepo;
            _mapper = mapper;
        }

        public async Task<PagedResult<AdminPayoutTransactionDto>> GetPayoutTransactionsAsync(PayoutTransactionQueryDto query)
        {
            var pagedTransactions = await _txRepo.GetPagedAsync(query);
            var items = _mapper.Map<List<AdminPayoutTransactionDto>>(pagedTransactions.Items);
            return new PagedResult<AdminPayoutTransactionDto>(items, pagedTransactions.TotalCount, pagedTransactions.PageNumber, pagedTransactions.PageSize);
        }

        public async Task<AdminPayoutTransactionSummaryDto> GetPayoutTransactionSummaryAsync(PayoutTransactionQueryDto query)
        {
            return await _txRepo.GetSummaryAsync(query);
        }

        public async Task<AdminPayoutTransactionDetailDto?> GetPayoutTransactionDetailAsync(Guid id)
        {
            var tx = await _txRepo.GetByIdWithDetailsAsync(id);
            if (tx == null) return null;

            return _mapper.Map<AdminPayoutTransactionDetailDto>(tx);
        }

        public async Task<PagedResult<AdminPayoutBatchDto>> GetPayoutBatchesAsync(AdminPayoutBatchQueryDto query)
        {
            var pagedBatches = await _batchRepo.GetPagedAsync(query);

            var items = _mapper.Map<List<AdminPayoutBatchDto>>(pagedBatches.Items);
            return new PagedResult<AdminPayoutBatchDto>(items, pagedBatches.TotalCount, pagedBatches.PageNumber, pagedBatches.PageSize);
        }

        public async Task<AdminPayoutBatchSummaryDto> GetPayoutSummaryAsync(AdminPayoutBatchQueryDto query)
        {
            return await _batchRepo.GetSummaryAsync(query);
        }

        public async Task<AdminPayoutBatchDto?> GetPayoutBatchDetailAsync(Guid id)
        {
            var batch = await _batchRepo.GetByIdWithTransactionsAsync(id);
            if (batch == null) return null;

            var dto = _mapper.Map<AdminPayoutBatchDto>(batch);
            dto.Transactions = _mapper.Map<List<AdminPayoutTransactionDto>>(batch.Transactions);
            return dto;
        }

        public async Task<List<AdminSellerPayableSummaryDto>> GetSellerPayableSummaryAsync(DateTime start, DateTime end)
        {
            var eligibleOrders = await _subOrderRepo.GetEligibleForPayoutAsync(start, end);

            var overview = eligibleOrders
                .GroupBy(o => o.StoreId)
                .Select(g => {
                    var store = g.First().Store;
                    return new AdminSellerPayableSummaryDto
                    {
                        StoreId = g.Key,
                        StoreName = store.StoreName,
                        LogoUrl = store.LogoUrl,
                        OrderCount = g.Count(),
                        TotalGrossAmount = g.Sum(o => o.Subtotal),
                        TotalCommissionAmount = g.Sum(o => o.CommissionAmount),
                        TotalNetAmount = g.Sum(o => o.NetAmount)
                    };
                })
                .OrderByDescending(x => x.TotalNetAmount)
                .ToList();

            return overview;
        }
    }
}
