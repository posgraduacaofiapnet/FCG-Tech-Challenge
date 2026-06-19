using FCG.Application.DTOs;
using FCG.Domain.Enums;
using FluentValidation;

namespace FCG.Application.Validators;

public class UpdateUserRoleValidator : AbstractValidator<UpdateUserRoleDto>
{
    public UpdateUserRoleValidator()
    {
        RuleFor(user => user.Role)
            .NotEmpty().WithMessage("Role e obrigatoria.")
            .Must(role => Enum.TryParse<Role>(role, ignoreCase: true, out _))
            .WithMessage("Role informada e invalida.");
    }
}
