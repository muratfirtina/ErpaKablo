using System.Security.Claims;
using Application.Consts;
using Application.CustomAttributes;
using Application.Enums;
using Application.Features.Products.Commands.Create;
using Application.Features.Products.Commands.Delete;
using Application.Features.Products.Commands.Update;
using Application.Features.Products.Dtos.FilterDto;
using Application.Features.Products.Queries.GetByDynamic;
using Application.Features.Products.Queries.GetById;
using Application.Features.Products.Queries.GetList;
using Application.Features.Products.Queries.GetMostLikedProducts;
using Application.Features.Products.Queries.GetRandomProductsByProductId;
using Application.Features.Products.Queries.GetRandomProductsForBrand;
using Application.Features.Products.Queries.GetRandoms;
using Application.Features.Products.Queries.SearchAndFilter;
using Application.Features.Products.Queries.SearchAndFilter.Filter;
using Application.Features.Products.Queries.SearchAndFilter.Filter.GetAvailableFilter;
using Application.Features.Products.Queries.SearchAndFilter.Search;
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
        
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string searchTerm, [FromQuery] PageRequest pageRequest)
        {
            GetListResponse<SearchProductQueryResponse> response = await Mediator.Send(new SearchProductQuery { SearchTerm = searchTerm, PageRequest = pageRequest });
            return Ok(response);
        }
        
        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromQuery] PageRequest pageRequest, [FromBody] FilterProductQuery filterQuery)
        {
            var request = new FilterProductWithPaginationQuery
            {
                SearchTerm = filterQuery.SearchTerm,
                Filters = filterQuery.Filters,
                PageRequest = pageRequest,
                SortOrder = filterQuery.SortOrder ?? "default"
            };
            GetListResponse<FilterProductQueryResponse> response = await Mediator.Send(request);
            return Ok(response);
        }
        
        [HttpGet("filters")]
        public async Task<ActionResult<List<FilterGroupDto>>> GetAvailableFilters([FromQuery] string searchTerm)
        {
            List<FilterGroupDto> filters = await Mediator.Send(new GetAvailableFiltersQuery { SearchTerm = searchTerm });
            return Ok(filters);
        }
        
        [HttpGet("GetRandomsByProductId/{productId}")]
        public async Task<IActionResult> GetRandomsByProductId([FromRoute]string productId)
        {
            GetListResponse<GetRandomProductsByProductIdQueryResponse> response = await Mediator.Send(new GetRandomProductsByProductIdQuery { ProductId = productId });
            return Ok(response);
        }
        
        [HttpGet("most-liked")]
        public async Task<IActionResult> GetMostLikedProducts([FromQuery] int count = 10)
        {
            var query = new GetMostLikedProductQuery() { Count = count };
            var products = await Mediator.Send(query);
            return Ok(products);
        }
        
        [HttpGet("GetRandomsForBrand/{productId}")]
        public async Task<IActionResult> GetRandomsForBrand([FromRoute]string productId)
        {
            GetListResponse<GetRandomProductsForBrandByProductIdQueryResponse> response = await Mediator.Send(new GetRandomProductsForBrandByProductIdQuery { ProductId = productId });
            return Ok(response);
        }
    }
}
