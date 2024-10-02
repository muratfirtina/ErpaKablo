using Application.Consts;
using Application.CustomAttributes;
using Application.Enums;
using Application.Features.ProductLikes.Commands.AddProductLike;
using Application.Features.ProductLikes.Queries;
using Application.Features.ProductLikes.Queries.GetProductsUserLiked;
using Application.Features.ProductLikes.Queries.GetUserLikedProductIds;
using Application.Features.ProductLikes.Queries.IsProductLiked;
using Core.Application.Requests;
using Core.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Admin")]
    public class ProductLikesController : BaseController
    {
        [HttpPost]
        [AuthorizeDefinition(ActionType = ActionType.Writing, Definition = "Create Product Like", Menu = AuthorizeDefinitionConstants.ProductLikes)]
        public async Task<IActionResult> Add([FromBody] AddProductLikeCommand addProductLikeCommand)
        {
            AddProductLikeResponse response = await Mediator.Send(addProductLikeCommand);
            return Created(uri: "", response);
        }
        
        [HttpPost("getProductsUserLiked")]
        [AuthorizeDefinition(ActionType = ActionType.Reading, Definition = "Get Products User Liked", Menu = AuthorizeDefinitionConstants.ProductLikes)]
        public async Task<IActionResult> Get([FromQuery] PageRequest pageRequest)
        {
            GetListResponse<GetUserLikedProductsQueryResponse> response = await Mediator.Send(new GetUserLikedProductsQuery { PageRequest = pageRequest });
            return Ok(response);
        }
        [HttpGet("liked-product-ids")]
        [AuthorizeDefinition(ActionType = ActionType.Reading, Definition = "Get User Liked Product Ids", Menu = AuthorizeDefinitionConstants.ProductLikes)]
        public async Task<IActionResult> GetUserLikedProductIds([FromQuery] string productIds)
        {
            var query = new GetUserLikedProductIdsQuery { SearchProductIds = productIds};
            var response = await Mediator.Send(query);
            return Ok(response);
        }
        [HttpGet("isLiked/{productId}")]
        [AuthorizeDefinition(ActionType = ActionType.Reading, Definition = "Is Liked", Menu = AuthorizeDefinitionConstants.ProductLikes)]
        public async Task<IActionResult> IsLiked([FromRoute] string productId)
        {
            var query = new IsProductLikedQuery { ProductId = productId };
            var response = await Mediator.Send(query);
            return Ok(response);
        }
            
            
    }
}
