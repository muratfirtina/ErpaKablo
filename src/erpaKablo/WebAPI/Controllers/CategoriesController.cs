using Application.Features.Categories.Commands.Create;
using Application.Features.Categories.Commands.Delete;
using Application.Features.Categories.Commands.Update;
using Application.Features.Categories.Queries.GetByDynamic;
using Application.Features.Categories.Queries.GetById;
using Application.Features.Categories.Queries.GetList;
using Application.Features.Categories.Queries.GetMainCategories;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Dynamic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
        {
            GetListResponse<GetAllCategoryQueryResponse> response = await Mediator.Send(new GetAllCategoryQuery { PageRequest = pageRequest });
            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            GetByIdCategoryResponse response = await Mediator.Send(new GetByIdCategoryQuery { Id = id });
            return Ok(response);
        }
        [HttpPost]
        public async Task<IActionResult> Add([FromForm] CreateCategoryCommand createCategoryCommand)
        {
            CreatedCategoryResponse response = await Mediator.Send(createCategoryCommand);

            return Created(uri: "", response);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            DeletedCategoryResponse response = await Mediator.Send(new DeleteCategoryCommand { Id = id });
            return Ok(response);
        }
        [HttpPut]
        public async Task<IActionResult> Update([FromForm] UpdateCategoryCommand updateCategoryCommand)
        {
            UpdatedCategoryResponse response = await Mediator.Send(updateCategoryCommand);
            return Ok(response);
        }

        [HttpPost("GetList/ByDynamic")]
        public async Task<IActionResult> GetListByDynamic([FromQuery] PageRequest pageRequest, [FromBody] DynamicQuery? dynamicQuery = null)
        {
            GetListResponse<GetListCategoryByDynamicDto> response = await Mediator.Send(new GetListCategoryByDynamicQuery { PageRequest = pageRequest, DynamicQuery = dynamicQuery });
            return Ok(response);
        }
        [HttpGet("GetMainCategories")]
        public async Task<IActionResult> GetMainCategories([FromQuery] PageRequest pageRequest)
        {
            GetListResponse<GetMainCategoriesResponse> response = await Mediator.Send(new GetMainCategoiesQuery { PageRequest = pageRequest });
            return Ok(response);
        }
    }
}
