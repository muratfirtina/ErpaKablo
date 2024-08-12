using Application.Features.Roles.Commands.CreateRole;
using Application.Features.Roles.Commands.DeleteRole;
using Application.Features.Roles.Commands.UpdateRole;
using Application.Features.Roles.Queries.GetRoleById;
using Application.Features.Roles.Queries.GetRoles;
using Application.Features.Roles.Queries.GetUsersByRoleId;
using Core.Application.Requests;
using Core.Application.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
        {
            GetListResponse<GetAllRolesQueryResponse> response = await Mediator.Send(new GetAllRolesQuery { PageRequest = pageRequest });
            return Ok(response);
        }
        
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleCommand createRoleCommand)
        {
            CreatedRoleResponse response = await Mediator.Send(createRoleCommand);
            return Created(uri: "", response);
        }

        [HttpDelete("{roleId}")]
        public async Task<IActionResult> Delete([FromRoute] DeleteRoleCommand deleteRoleCommand)
        {
            DeletedRoleResponse response = await Mediator.Send(deleteRoleCommand);
            return Ok(response);
        }
        
        [HttpPut("{roleId}")]
        public async Task<IActionResult> Update([FromBody,FromRoute] UpdateRoleCommand updateRoleCommand)
        {
            UpdatedRoleResponse response = await Mediator.Send(updateRoleCommand);
            return Ok(response);
        }
        
        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetById([FromRoute] string roleId)
        {
            GetRoleByIdQueryResponse response = await Mediator.Send(new GetRoleByIdQuery { RoleId = roleId });
            return Ok(response);
        }
        
        [HttpGet("{roleId}/users")]
        public async Task<ActionResult> GetUsersByRoleId([FromRoute] string roleId)
        {
            GetListResponse<GetUsersByRoleIdQueryResponse> response = await Mediator.Send(new GetUsersByRoleIdQuery{RoleId = roleId});
            return Ok(response);
        }
        
    }
}
