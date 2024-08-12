using Application.Repositories;
using AutoMapper;
using Domain;
using Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Carts.Commands.AddItemToCart;

public class CreateCartCommand : IRequest<CreatedCartResponse>
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public bool IsChecked { get; set; } = true;

    public class CreateCartCommandHandler : IRequestHandler<CreateCartCommand, CreatedCartResponse>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public CreateCartCommandHandler(
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IProductRepository productRepository,
            IHttpContextAccessor httpContextAccessor,
            UserManager<AppUser> userManager,
            IMapper mapper)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<CreatedCartResponse> Handle(CreateCartCommand request, CancellationToken cancellationToken)
        {
            var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                throw new Exception("User not found.");
            }

            var user = await _userManager.Users
                .Include(u => u.Carts)
                .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var activeCart = user.Carts.FirstOrDefault(c => c.Order == null);
            if (activeCart == null)
            {
                activeCart = new Cart();
                user.Carts.Add(activeCart);
                await _cartRepository.AddAsync(activeCart);
            }

            var product = await _productRepository.GetAsync(p => p.Id == (request.ProductId));
            if (product == null)
            {
                throw new Exception("Product not found.");
            }

            if (product.Stock <= 0)
            {
                throw new Exception("Product stock is not enough.");
            }

            var cartItem = await _cartItemRepository.GetAsync(
                ci => ci.CartId == activeCart.Id && ci.ProductId == request.ProductId,
                cancellationToken: cancellationToken
            );

            if (cartItem != null)
            {
                if (cartItem.Quantity < product.Stock)
                {
                    cartItem.Quantity++;
                    cartItem.IsChecked = request.IsChecked;
                    await _cartItemRepository.UpdateAsync(cartItem);
                }
                else
                {
                    throw new Exception("Product stock is not enough.");
                }
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = activeCart.Id,
                    ProductId = request.ProductId,
                    Quantity = 1,
                    IsChecked = request.IsChecked
                };
                await _cartItemRepository.AddAsync(cartItem);
            }

            await _cartRepository.AddAsync(activeCart);

            var response = _mapper.Map<CreatedCartResponse>(activeCart);
            return response;
        }
    }
}