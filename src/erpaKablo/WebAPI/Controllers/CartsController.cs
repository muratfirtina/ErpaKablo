using Application.Consts;
using Application.CustomAttributes;
using Application.Enums;
using Application.Features.Carts.Commands.AddItemToCart;
using Application.Features.Carts.Commands.RemoveCartItem;
using Application.Features.Carts.Commands.UpdateCartItem;
using Application.Features.Carts.Commands.UpdateQuantity;
using Application.Features.Carts.Queries.GetList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Admin")]
    public class CartsController : BaseController
    {

        [HttpPost]
        [AuthorizeDefinition(ActionType = ActionType.Writing, Definition = "Add Item To Cart", Menu = AuthorizeDefinitionConstants.Carts)]
        public async Task<IActionResult> AddItemToCart([FromBody] CreateCartCommand createCartCommand)
        {
            CreatedCartResponse response = await Mediator.Send(createCartCommand);
            return Ok(response);
        }
        
        [HttpGet]
        [AuthorizeDefinition(ActionType = ActionType.Reading, Definition = "Get Cart Items", Menu = AuthorizeDefinitionConstants.Carts)]
        public async Task<IActionResult> GetCartItems([FromQuery]GetCartItemsQuery getCartItemsQuery)
        {
            List<GetCartItemsQueryResponse> response = await Mediator.Send(getCartItemsQuery);
            return Ok(response);
        }
        
        
        [HttpPut]
        [AuthorizeDefinition(ActionType = ActionType.Updating, Definition = "update quantity", Menu = AuthorizeDefinitionConstants.Carts)]
        public async Task<IActionResult> UpdateQuantity(UpdateQuantityCommand updateQuantityCommand)
        {
            UpdateQuantityResponse response = await Mediator.Send(updateQuantityCommand);
            return Ok(response);
        }
        [HttpDelete("{CartItemId}")]
        [AuthorizeDefinition(ActionType = ActionType.Deleting, Definition = "Remove Cart Items", Menu = AuthorizeDefinitionConstants.Carts)]
        public async Task<IActionResult> RemoveCartItem([FromRoute]RemoveCartItemCommand removeCartItemCommand)
        {
            RemoveCartItemResponse response = await Mediator.Send(removeCartItemCommand);
            return Ok(response);
        }
        [HttpPut("UpdateCartItem")]
        [AuthorizeDefinition(ActionType = ActionType.Updating, Definition = "Update Cart Item isChecked", Menu = AuthorizeDefinitionConstants.Carts)]
        public async Task<IActionResult> UpdateCartItem(UpdateCartItemCommand updateCartItemCommand)
        {
            UpdateCartItemResponse response = await Mediator.Send(updateCartItemCommand);
            return Ok(response);
        }
    }
    
}
