using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Features.Admin
{
    public class AdminOrderService : IAdminOrderService
    {
        private const int ExportPageSize = 500;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public AdminOrderService(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<AdminOrderDto>> GetOrdersAsync(AdminOrderQueryDto queryDto)
        {
            var pagedOrders = await _orderRepository.GetAdminOrdersAsync(queryDto);
            var items = _mapper.Map<List<AdminOrderDto>>(pagedOrders.Items);
            return new PagedResult<AdminOrderDto>(items, pagedOrders.TotalCount, pagedOrders.PageNumber, pagedOrders.PageSize);
        }

        public async Task<AdminCsvFileDto> ExportOrdersCsvAsync(
            AdminOrderQueryDto queryDto,
            CancellationToken ct = default)
        {
            var exportQuery = CloneForExport(queryDto);
            var orders = new List<AdminOrderDto>();

            while (true)
            {
                ct.ThrowIfCancellationRequested();
                var page = await _orderRepository.GetAdminOrdersAsync(exportQuery);
                orders.AddRange(_mapper.Map<List<AdminOrderDto>>(page.Items));

                if (page.Items.Count == 0 || orders.Count >= page.TotalCount)
                {
                    break;
                }

                exportQuery.PageNumber++;
            }

            return AdminCsvExporter.ExportOrders(orders, DateTime.UtcNow);
        }

        public async Task<AdminOrderDetailDto?> GetOrderDetailAsync(Guid id)
        {
            var order = await _orderRepository.GetAdminOrderDetailAsync(id);
            if (order == null) return null;

            return _mapper.Map<AdminOrderDetailDto>(order);
        }

        public async Task<AdminOrderStatsDto> GetOrderStatsAsync()
        {
            return await _orderRepository.GetAdminOrderStatsAsync();
        }

        private static AdminOrderQueryDto CloneForExport(AdminOrderQueryDto source) =>
            new()
            {
                SearchTerm = source.SearchTerm,
                IsPaid = source.IsPaid,
                PaymentMethod = source.PaymentMethod,
                StoreId = source.StoreId,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                DateRange = source.DateRange,
                MinPrice = source.MinPrice,
                MaxPrice = source.MaxPrice,
                SortBy = source.SortBy,
                SortDirection = source.SortDirection,
                PageNumber = 1,
                PageSize = ExportPageSize
            };
    }
}
