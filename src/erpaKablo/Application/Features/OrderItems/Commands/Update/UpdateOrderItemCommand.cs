using Application.Repositories;
using Domain;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.OrderItems.Commands.Update
{
    public class UpdateOrderItemCommand : IRequest<bool>
    {
        public string Id { get; set; }  // OrderItem ID
        public int Quantity { get; set; }  // Güncellenecek miktar

        public class UpdateOrderItemCommandHandler : IRequestHandler<UpdateOrderItemCommand, bool>
        {
            private readonly IOrderItemRepository _orderItemRepository;

            public UpdateOrderItemCommandHandler(IOrderItemRepository orderItemRepository)
            {
                _orderItemRepository = orderItemRepository;
            }

            public async Task<bool> Handle(UpdateOrderItemCommand request, CancellationToken cancellationToken)
            {
                // Stok miktarını kontrol etmek ve miktarı güncellemek için repository kullanılır
                var result = await _orderItemRepository.UpdateOrderItemQuantityAsync(request.Id, request.Quantity);

                // Eğer işlem başarılı olduysa true döner, değilse false döner
                return result;
            }
        }
    }
}