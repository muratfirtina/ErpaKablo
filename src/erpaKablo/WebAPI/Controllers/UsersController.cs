using Application.Consts;
using Application.CustomAttributes;
using Application.Enums;
using Application.Features.Users.Commands.AssignRoleToUser;
using Application.Features.Users.Commands.CreateUser;
using Application.Features.Users.Commands.LoginUser;
using Application.Features.Users.Commands.UpdateForgetPassword;
using Application.Features.Users.Queries.GetAllUsers;
using Application.Features.Users.Queries.GetByDynamic;
using Application.Features.Users.Queries.GetRolesToUser;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Dynamic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseController
    {

        [HttpGet]
        //[Authorize(AuthenticationSchemes = "Admin")]
        //[AuthorizeDefinition(ActionType = ActionType.Reading, Definition = "Get All Users")]
        public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
        {
            GetListResponse<GetAllUsersQueryResponse> response = await Mediator.Send(new GetAllUsersQuery { PageRequest = pageRequest });
            return Ok(response);
        }
        
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserCommand createUserCommand)
        {
            CreatedUserResponse response = await Mediator.Send(createUserCommand);
            return Created(uri: "", response);
        }
        
        [HttpPost("update-forgot-password")]
        public async Task<IActionResult>UpdateForgotPassword(UpdateForgotPasswordRequest updateForgotPasswordRequest)
        {
            var response = await Mediator.Send(updateForgotPasswordRequest);
            return Ok(response);
        }
        
        [HttpPost("assign-role-to-user")]
        [Authorize(AuthenticationSchemes = "Admin")]
        [AuthorizeDefinition(ActionType = ActionType.Updating, Definition = "Assign Role To User", Menu = AuthorizeDefinitionConstants.Users)]
        public async Task<IActionResult> AssignRoleToUser(AssignRoleToUserRequest assignRoleToUserRequest)
        {
            var response = await Mediator.Send(assignRoleToUserRequest);
            return Ok(response);
        }
        
        [HttpGet("get-roles-to-user/{UserId}")]
        //[Authorize(AuthenticationSchemes = "Admin")]
        //[AuthorizeDefinition(ActionType = ActionType.Reading, Definition = "Get Roles To User", Menu = AuthorizeDefinitionConstants.Users)]
        public async Task<IActionResult> GetRolesToUser([FromRoute]GetRolesToUserQuery getRolesToUserQuery)
        {
            var response = await Mediator.Send(getRolesToUserQuery);
            return Ok(response);
        }
        
        [HttpPost("GetList/ByDynamic")]
        //[Authorize(AuthenticationSchemes = "Admin")]
        //[AuthorizeDefinition(ActionType = ActionType.Reading, Definition = "Get List User By Dynamic", Menu = AuthorizeDefinitionConstants.Users)]
        public async Task<IActionResult> GetListByDynamic([FromQuery] PageRequest pageRequest, [FromBody] DynamicQuery? dynamicQuery = null)
        {
            GetListResponse<GetListUserByDynamicQueryResponse> response = await Mediator.Send(new GetListUserByDynamicQuery { DynamicQuery = dynamicQuery, PageRequest = pageRequest });
            return Ok(response);
        }
        
    }
}
