using Application.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.OrderItems.Commands.Delete
{
    public class DeleteOrderItemCommand : IRequest<bool>
    {
        public string Id { get; set; }  // Silinecek OrderItem ID'si

        public class DeleteOrderItemCommandHandler : IRequestHandler<DeleteOrderItemCommand, bool>
        {
            private readonly IOrderItemRepository _orderItemRepository;

            public DeleteOrderItemCommandHandler(IOrderItemRepository orderItemRepository)
            {
                _orderItemRepository = orderItemRepository;
            }

            public async Task<bool> Handle(DeleteOrderItemCommand request, CancellationToken cancellationToken)
            {
                // OrderItem'ı silmek için repository metodunu kullan
                var result = await _orderItemRepository.RemoveOrderItemAsync(request.Id);

                // İşlem başarılıysa true, başarısızsa false döner
                return result;
            }
        }
    }
}