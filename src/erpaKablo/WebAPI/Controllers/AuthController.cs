using Application.Features.Users.Commands.LoginUser;
using Application.Features.Users.Commands.PasswordReset;
using Application.Features.Users.Commands.RefreshTokenLogin;
using Application.Features.Users.Commands.VerifyResetPasswordToken;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        [HttpPost("[action]")]
        public async Task<IActionResult> Login(LoginUserRequest loginUserRequest)
        {
            var response = await Mediator.Send(loginUserRequest);
            return Ok(response);
        }
        
        [HttpPost("[action]")]
        public async Task<IActionResult> RefreshTokenLogin([FromBody]RefreshTokenLoginRequest refreshTokenLoginRequest)
        {
            var response = await Mediator.Send(refreshTokenLoginRequest);
            return Ok(response);
        }
        
        [HttpPost("password-reset")]
        public async Task<IActionResult> PasswordReset(PasswordResetRequest passwordResetRequest)
        {
            var response = await Mediator.Send(passwordResetRequest);
            return Ok(response);
        }
        
        [HttpPost("verify-reset-password-token")]
        public async Task<IActionResult> VerifyResetPasswordToken([FromBody]VerifyResetPasswordTokenRequest verifyResetPasswordTokenRequest)
        {
            var response = await Mediator.Send(verifyResetPasswordTokenRequest);
            return Ok(response);
        }
    }
}
