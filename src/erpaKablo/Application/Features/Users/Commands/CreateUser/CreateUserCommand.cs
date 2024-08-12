using Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.Commands.CreateUser;

public class CreateUserCommand : IRequest<CreatedUserResponse>
{
    public string NameSurname { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreatedUserResponse>
    {
        private readonly UserManager<AppUser> _userManager;

        public CreateUserCommandHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<CreatedUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _userManager.CreateAsync(new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                NameSurname = request.NameSurname,
                UserName = request.UserName,
                Email = request.Email
            }, request.Password);
            CreatedUserResponse response = new CreatedUserResponse { IsSuccess = result.Succeeded };
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                await _userManager.AddToRoleAsync(user, "User");

                response.Message = "Created successfully.";
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    response.Message += $"{error.Code} - {error.Description}\n";
                }
            }
            return response;
        }
    }
}