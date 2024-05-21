using Application.Features.ProductFeatures.Commands.Create;
using Application.Features.ProductFeatures.Commands.Delete;
using Application.Features.ProductFeatures.Commands.Update;
using Application.Features.ProductFeatures.Queries.GetByDynamic;
using Application.Features.ProductFeatures.Queries.GetById;
using Application.Features.ProductFeatures.Queries.GetList;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Dynamic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductFeaturesController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
        {
            GetListResponse<GetAllProductFeatureQueryResponse> response = await Mediator.Send(new GetAllProductFeatureQuery { PageRequest = pageRequest });
            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            GetByIdProductFeatureResponse response = await Mediator.Send(new GetByIdProductFeatureQuery { Id = id });
            return Ok(response);
        }
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateProductFeatureCommand createProductFeatureCommand)
        {
            CreatedProductFeatureResponse response = await Mediator.Send(createProductFeatureCommand);

            return Created(uri: "", response);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            DeletedProductFeatureResponse response = await Mediator.Send(new DeleteProductFeatureCommand { Id = id });
            return Ok(response);
        }
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateProductFeatureCommand updateProductFeatureCommand)
        {
            UpdatedProductFeatureResponse response = await Mediator.Send(updateProductFeatureCommand);
            return Ok(response);
        }
        [HttpPost("GetList/ByDynamic")]
        public async Task<IActionResult> GetListByDynamic([FromQuery] PageRequest pageRequest, [FromBody] DynamicQuery? dynamicQuery = null)
        {
            GetListResponse<GetListProductFeatureByDynamicDto> response = await Mediator.Send(new GetListProductFeatureByDynamicQuery { PageRequest = pageRequest, DynamicQuery = dynamicQuery });
            return Ok(response);
        }
    }
}
