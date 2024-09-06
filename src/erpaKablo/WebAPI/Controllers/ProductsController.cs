using Application.Consts;
using Application.CustomAttributes;
using Application.Enums;
using Application.Features.Products.Commands.Create;
using Application.Features.Products.Commands.Delete;
using Application.Features.Products.Commands.Update;
using Application.Features.Products.Queries.GetByDynamic;
using Application.Features.Products.Queries.GetById;
using Application.Features.Products.Queries.GetList;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Dynamic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
        {
            GetListResponse<GetAllProductQueryResponse> response = await Mediator.Send(new GetAllProductQuery { PageRequest = pageRequest });
            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            GetByIdProductResponse response = await Mediator.Send(new GetByIdProductQuery { Id = id });
            return Ok(response);
        }
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Authorize(AuthenticationSchemes = "Admin")]
        [AuthorizeDefinition(ActionType = ActionType.Writing, Definition = "Create Product", Menu = AuthorizeDefinitionConstants.Products)]
        public async Task<IActionResult> Add([FromForm] CreateProductCommand createProductCommand)
        {
            CreatedProductResponse response = await Mediator.Send(createProductCommand);
            return Created(uri: "", response);
        }
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "Admin")]
        [AuthorizeDefinition(ActionType = ActionType.Deleting, Definition = "Delete Product", Menu = AuthorizeDefinitionConstants.Products)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            DeletedProductResponse response = await Mediator.Send(new DeleteProductCommand { Id = id });
            return Ok(response);
        }
        [HttpPut]
        [Consumes("multipart/form-data")]
        [Authorize(AuthenticationSchemes = "Admin")]
        [AuthorizeDefinition(ActionType = ActionType.Updating, Definition = "Update Product", Menu = AuthorizeDefinitionConstants.Products)]
        public async Task<IActionResult> Update([FromForm] UpdateProductCommand updateProductCommand)
        {
            UpdatedProductResponse response = await Mediator.Send(updateProductCommand);
            return Ok(response);
        }
        [HttpPost("GetList/ByDynamic")]
        public async Task<IActionResult> GetListByDynamic([FromQuery] PageRequest pageRequest, [FromBody] DynamicQuery? dynamicQuery = null)
        {
            GetListResponse<GetListProductByDynamicDto> response = await Mediator.Send(new GetListProductByDynamicQuery { PageRequest = pageRequest, DynamicQuery = dynamicQuery });
            return Ok(response);
        }
        
        [HttpPost("multiple")]
        [Consumes("multipart/form-data")]
        [Authorize(AuthenticationSchemes = "Admin")]
        [AuthorizeDefinition(ActionType = ActionType.Writing, Definition = "Create Multiple Products", Menu = AuthorizeDefinitionConstants.Products)]
        public async Task<IActionResult> CreateMultiple([FromForm] CreateMultipleProductsCommand createMultipleProductsCommand)
        {
            List<CreatedProductResponse> response = await Mediator.Send(createMultipleProductsCommand);
            return Created(uri: "", response);
        }
        
        [HttpGet("GetRandomProductsByCategory/{categoryId}")]
        public async Task<IActionResult> GetRandomProductsByCategory(string categoryId, [FromQuery] int count = 4)
        {
            var products = await Mediator.Send(new GetRandomProductsByCategoryQuery { CategoryId = categoryId, Count = count });
            return Ok(products);
        }
    }
}
