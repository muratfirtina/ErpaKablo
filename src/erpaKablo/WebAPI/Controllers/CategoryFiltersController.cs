using Application.Features.CategoryFilters.Commands.Create;
using Application.Features.CategoryFilters.Commands.Delete;
using Application.Features.CategoryFilters.Commands.Update;
using Application.Features.CategoryFilters.Queries.GetByDynamic;
using Application.Features.CategoryFilters.Queries.GetById;
using Application.Features.CategoryFilters.Queries.GetList;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Dynamic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryFiltersController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
        {
            GetListResponse<GetAllCategoryFilterQueryResponse> response = await Mediator.Send(new GetAllCategoryFilterQuery { PageRequest = pageRequest });
            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            GetByIdCategoryFilterResponse response = await Mediator.Send(new GetByIdCategoryFilterQuery { Id = id });
            return Ok(response);
        }
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateCategoryFilterCommand createCategoryFilterCommand)
        {
            CreatedCategoryFilterResponse response = await Mediator.Send(createCategoryFilterCommand);

            return Created(uri: "", response);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            DeletedCategoryFilterResponse response = await Mediator.Send(new DeleteCategoryFilterCommand { Id = id });
            return Ok(response);
        }
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCategoryFilterCommand updateCategoryFilterCommand)
        {
            UpdatedCategoryFilterResponse response = await Mediator.Send(updateCategoryFilterCommand);
            return Ok(response);
        }
        [HttpPost("GetList/ByDynamic")]
        public async Task<IActionResult> GetListByDynamic([FromQuery] PageRequest pageRequest, [FromBody] DynamicQuery? dynamicQuery = null)
        {
            GetListResponse<GetListCategoryFilterByDynamicDto> response = await Mediator.Send(new GetListCategoryFilterByDynamicQuery { PageRequest = pageRequest, DynamicQuery = dynamicQuery });
            return Ok(response);
        }
    }
}
