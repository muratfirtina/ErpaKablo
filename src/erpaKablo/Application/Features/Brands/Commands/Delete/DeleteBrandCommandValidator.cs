using FluentValidation;

namespace Application.Features.Brands.Commands.Delete;

public class DeleteBrandCommandValidator : AbstractValidator<DeleteBrandCommand>
{
    public DeleteBrandCommandValidator()
    {
        RuleFor(p => p.Id).NotEmpty();
    }
}