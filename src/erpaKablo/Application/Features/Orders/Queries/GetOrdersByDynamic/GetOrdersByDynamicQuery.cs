using Application.Extensions;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Dynamic;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries.GetOrdersByDynamic;

public class GetOrdersByDynamicQuery : IRequest<GetListResponse<GetOrdersByDynamicQueryResponse>>
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery DynamicQuery { get; set; }
    
    public class GetOrdersByDynamicQueryHandler : IRequestHandler<GetOrdersByDynamicQuery, GetListResponse<GetOrdersByDynamicQueryResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;

        public GetOrdersByDynamicQueryHandler(IOrderRepository orderRepository, IMapper mapper, IStorageService storageService)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<GetListResponse<GetOrdersByDynamicQueryResponse>> Handle(GetOrdersByDynamicQuery request, CancellationToken cancellationToken)
        {
            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                var allOrders = await _orderRepository.GetAllByDynamicAsync(
                    request.DynamicQuery,
                    include: q => q
                        .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductImageFiles.Where(pif => pif.Showcase))
                        .Include(o => o.User),
                    cancellationToken: cancellationToken);

                GetListResponse<GetOrdersByDynamicQueryResponse> ordersDtos = _mapper.Map<GetListResponse<GetOrdersByDynamicQueryResponse>>(allOrders);
                
                ordersDtos.Items.SetImageUrl(_storageService);

                return ordersDtos;
            }
            else
            {
                var orders = await _orderRepository.GetListByDynamicAsync(
                    request.DynamicQuery,
                    include: q => q
                        .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductImageFiles.Where(pif => pif.Showcase))
                        .Include(o => o.User),

                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    cancellationToken: cancellationToken);

                GetListResponse<GetOrdersByDynamicQueryResponse> ordersDtos = _mapper.Map<GetListResponse<GetOrdersByDynamicQueryResponse>>(orders);

                ordersDtos.Items.SetImageUrl(_storageService);

                return ordersDtos;
            }
        }
    }
}