using Application.CustomAttributes;
using Application.Enums;
using Application.Features.Users.Commands.AssignRoleToUser;
using Application.Features.Users.Commands.CreateUser;
using Application.Features.Users.Commands.LoginUser;
using Application.Features.Users.Commands.UpdateForgetPassword;
using Application.Features.Users.Queries.GetAllUsers;
using Application.Features.Users.Queries.GetRolesToUser;
using Core.Application.Requests;
using Core.Application.Responses;
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
        
        public async Task<IActionResult> AssignRoleToUser(AssignRoleToUserRequest assignRoleToUserRequest)
        {
            var response = await Mediator.Send(assignRoleToUserRequest);
            return Ok(response);
        }
        
        [HttpGet("get-roles-to-user/{UserId}")]
        
        public async Task<IActionResult> GetRolesToUser([FromRoute]GetRolesToUserQuery getRolesToUserQuery)
        {
            var response = await Mediator.Send(getRolesToUserQuery);
            return Ok(response);
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser(LoginUserRequest loginUserRequest)
        {
            var response = await Mediator.Send(loginUserRequest);
            return Ok(response);
        }
    }
}
