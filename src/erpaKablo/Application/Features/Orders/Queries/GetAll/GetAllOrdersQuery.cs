using Application.Repositories;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries.GetAll;

public class GetAllOrdersQuery : IRequest<GetListResponse<GetAllOrdersQueryResponse>>
{
    public PageRequest PageRequest { get; set; }
    
    public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, GetListResponse<GetAllOrdersQueryResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public GetAllOrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetAllOrdersQueryResponse>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            IPaginate<Order> orders = await _orderRepository.GetListAsync(
                include: o => o
                    .Include(o => o.OrderItems)
                    .Include(o => o.User),
                cancellationToken: cancellationToken);
            
            GetListResponse<GetAllOrdersQueryResponse> response = _mapper.Map<GetListResponse<GetAllOrdersQueryResponse>>(orders);
            return response;

        }
    }
}
