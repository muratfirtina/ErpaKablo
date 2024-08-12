using Application.Abstraction.Services.Configurations;
using Application.CustomAttributes;
using Application.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationServicesController : BaseController
    {
        private readonly IApplicationService _applicationService;

        public ApplicationServicesController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }
        [HttpGet]
        public IActionResult GetAuthorizeDefinitionEnpoints()
        {
            var datas = _applicationService.GetAuthorizeDefinitionEnpoints(typeof(Program));
            return Ok(datas);
        }
    }
}
