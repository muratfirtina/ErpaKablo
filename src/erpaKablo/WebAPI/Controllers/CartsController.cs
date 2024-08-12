using Application.Consts;
using Application.CustomAttributes;
using Application.Enums;
using Application.Features.Carts.Commands.AddItemToCart;
using Application.Features.Carts.Commands.RemoveCartItem;
using Application.Features.Carts.Commands.UpdateCartItem;
using Application.Features.Carts.Commands.UpdateQuantity;
using Application.Features.Carts.Queries.GetList;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : BaseController
    {

        [HttpPost]
        public async Task<IActionResult> AddItemToCart([FromBody] CreateCartCommand createCartCommand)
        {
            CreatedCartResponse response = await Mediator.Send(createCartCommand);
            return Ok(response);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetCartItems([FromQuery]GetCartItemsQuery getCartItemsQuery)
        {
            List<GetCartItemsQueryResponse> response = await Mediator.Send(getCartItemsQuery);
            return Ok(response);
        }
        
        
        [HttpPut]
        public async Task<IActionResult> UpdateQuantity(UpdateQuantityCommand updateQuantityCommand)
        {
            UpdateQuantityResponse response = await Mediator.Send(updateQuantityCommand);
            return Ok(response);
        }
        [HttpDelete("{CartItemId}")]
        public async Task<IActionResult> RemoveCartItem([FromRoute]RemoveCartItemCommand removeCartItemCommand)
        {
            RemoveCartItemResponse response = await Mediator.Send(removeCartItemCommand);
            return Ok(response);
        }
        [HttpPut("UpdateCartItem")]
        public async Task<IActionResult> UpdateCartItem(UpdateCartItemCommand updateCartItemCommand)
        {
            UpdateCartItemResponse response = await Mediator.Send(updateCartItemCommand);
            return Ok(response);
        }
    }
    
}
