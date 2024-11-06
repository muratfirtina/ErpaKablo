using Application.Abstraction.Services;
using Application.Features.Orders.Dtos;
using Application.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Orders.Commands.Create;

public class ConvertCartToOrderCommand : IRequest<ConvertCartToOrderCommandResponse>
{
    public string? AddressId { get; set; }
    public string? PhoneNumberId { get; set; }
    public string? Description { get; set; }

    public class ConvertCartToOrderCommandHandler : IRequestHandler<ConvertCartToOrderCommand, ConvertCartToOrderCommandResponse>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMailService _mailService;
        private readonly IMapper _mapper;

        public ConvertCartToOrderCommandHandler(IMapper mapper, IOrderRepository orderRepository, IMailService mailService)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _mailService = mailService;
        }

        public async Task<ConvertCartToOrderCommandResponse> Handle(ConvertCartToOrderCommand request, CancellationToken cancellationToken)
        {
            (bool succeeded, OrderDto orderDto) = await _orderRepository.ConvertCartToOrderAsync(
                request.AddressId,
                request.PhoneNumberId,
                request.Description
            );
    
            if (!succeeded || orderDto == null)
            {
                throw new Exception("Sepet siparişe dönüştürülemedi.");
            }

            /*await _mailService.SendCompletedOrderEmailAsync(
                orderDto.Email, 
                orderDto.OrderCode,
                orderDto.Description,
                orderDto.UserAddress,
                orderDto.OrderDate,
                orderDto.UserName,
                orderDto.OrderItems,
                orderDto.TotalPrice
            );*/

            return new ConvertCartToOrderCommandResponse
            {
                OrderId = orderDto.OrderId
            };
        }
    }
}
