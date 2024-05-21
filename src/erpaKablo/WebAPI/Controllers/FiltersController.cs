using Application.Features.Filters.Commands.Create;
using Application.Features.Filters.Commands.Delete;
using Application.Features.Filters.Commands.Update;
using Application.Features.Filters.Queries.GetByDynamic;
using Application.Features.Filters.Queries.GetById;
using Application.Features.Filters.Queries.GetList;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Dynamic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FiltersController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
        {
            GetListResponse<GetAllFilterQueryResponse> response = await Mediator.Send(new GetAllFilterQuery { PageRequest = pageRequest });
            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            GetByIdFilterResponse response = await Mediator.Send(new GetByIdFilterQuery { Id = id });
            return Ok(response);
        }
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateFilterCommand createFilterCommand)
        {
            CreatedFilterResponse response = await Mediator.Send(createFilterCommand);

            return Created(uri: "", response);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            DeletedFilterResponse response = await Mediator.Send(new DeleteFilterCommand { Id = id });
            return Ok(response);
        }
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateFilterCommand updateFilterCommand)
        {
            UpdatedFilterResponse response = await Mediator.Send(updateFilterCommand);
            return Ok(response);
        }
        [HttpPost("GetList/ByDynamic")]
        public async Task<IActionResult> GetListByDynamic([FromQuery] PageRequest pageRequest, [FromBody] DynamicQuery? dynamicQuery = null)
        {
            GetListResponse<GetListFilterByDynamicDto> response = await Mediator.Send(new GetListFilterByDynamicQuery { PageRequest = pageRequest, DynamicQuery = dynamicQuery });
            return Ok(response);
        }
    }
}
