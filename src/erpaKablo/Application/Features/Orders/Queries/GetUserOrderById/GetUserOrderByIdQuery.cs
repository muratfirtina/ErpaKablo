using Application.Extensions;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using MediatR;

namespace Application.Features.Orders.Queries.GetUserOrderById;

public class GetUserOrderByIdQuery : IRequest<GetUserOrderByIdQueryResponse>
{
    public string Id { get; set; }

    public class GetUserOrderByIdQueryHandler : IRequestHandler<GetUserOrderByIdQuery, GetUserOrderByIdQueryResponse>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;

        public GetUserOrderByIdQueryHandler(
            IOrderRepository orderRepository,
            IMapper mapper,
            IStorageService storageService)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<GetUserOrderByIdQueryResponse> Handle(GetUserOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetUserOrderByIdAsync(request.Id);
            if (order == null)
                throw new Exception("Order not found");

            var response = _mapper.Map<GetUserOrderByIdQueryResponse>(order);
            response.OrderItems.SetImageUrls(_storageService);

            return response;
        }
    }
}