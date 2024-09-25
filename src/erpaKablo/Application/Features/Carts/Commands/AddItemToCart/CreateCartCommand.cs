using Application.Features.Carts.Dtos;
using Application.Features.Carts.Queries.GetList;
using Application.Repositories;
using Application.Services;
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
    public CreateCartItemDto CreateCartItem { get; set; }

    public class CreateCartCommandHandler : IRequestHandler<CreateCartCommand, CreatedCartResponse>
    {
        private readonly ICartService _cartService;
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
            IMapper mapper, ICartService cartService)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _mapper = mapper;
            _cartService = cartService;
        }

        public async Task<CreatedCartResponse> Handle(CreateCartCommand request, CancellationToken cancellationToken)
        {
            await _cartService.AddItemToCartAsync(request.CreateCartItem);
            return new();
        }
    }
}