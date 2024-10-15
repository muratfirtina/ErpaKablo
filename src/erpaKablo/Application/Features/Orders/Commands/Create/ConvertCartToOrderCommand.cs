using Application.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Orders.Commands.Create;

public class ConvertCartToOrderCommand : IRequest<string>
{

    public class ConvertCartToOrderCommandHandler : IRequestHandler<ConvertCartToOrderCommand, string>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public ConvertCartToOrderCommandHandler(IMapper mapper, IOrderRepository orderRepository)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
        }

        public async Task<string> Handle(ConvertCartToOrderCommand request, CancellationToken cancellationToken)
        {
            // Siparişi dönüştür ve sipariş ID'sini döndür
            return await _orderRepository.ConvertCartToOrderAsync();
        }
    }
}