using Application.Abstraction.Services;
using Application.Features.Orders.Dtos;
using Application.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Orders.Commands.Create;

public class ConvertCartToOrderCommand : IRequest<string>
{

    public class ConvertCartToOrderCommandHandler : IRequestHandler<ConvertCartToOrderCommand, string>
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

        public async Task<string> Handle(ConvertCartToOrderCommand request, CancellationToken cancellationToken)
        {
            // Siparişi dönüştür ve sipariş ID'sini döndür
            (bool succeeded, OrderDto orderDto) = await _orderRepository.ConvertCartToOrderAsync();
    
            if (!succeeded || orderDto == null)
            {
                // Sipariş dönüşümünde bir hata varsa, uygun bir hata mesajı döndürebilir veya hata fırlatabilirsiniz.
                throw new Exception("Sepet siparişe dönüştürülemedi.");
            }

            // Sipariş tamamlandı e-postasını gönder
            await _mailService.SendCompletedOrderEmailAsync(orderDto.Email, orderDto.OrderCode, orderDto.Description, orderDto.UserAddress, orderDto.OrderDate, orderDto.UserName, orderDto.OrderItems, orderDto.TotalPrice);

            return orderDto.OrderId;
        }
    }
}