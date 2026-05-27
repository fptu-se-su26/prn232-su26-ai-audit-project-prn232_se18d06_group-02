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
    }
}
