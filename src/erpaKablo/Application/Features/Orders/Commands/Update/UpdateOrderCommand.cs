using Application.Repositories;
using Domain;
using Domain.Enum;
using MediatR;

namespace Application.Features.Orders.Commands.Update;

public class UpdateOrderCommand : IRequest<bool>
{
    public string Id { get; set; }
    public OrderStatus Status { get; set; }
    public decimal? TotalPrice { get; set; }
   

    public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, bool>
    {
        private readonly IOrderRepository _orderRepository;

        public UpdateOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<bool> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (order == null) throw new Exception("Order not found");

            order.Status = request.Status;
            order.TotalPrice = request.TotalPrice;
   

            await _orderRepository.UpdateAsync(order);
            return true;
        }
    }
}