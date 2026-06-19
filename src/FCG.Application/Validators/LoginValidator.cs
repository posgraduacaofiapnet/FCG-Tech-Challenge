using FCG.Application.DTOs;
using FluentValidation;

namespace FCG.Application.Validators;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email e obrigatorio.")
            .EmailAddress().WithMessage("Email invalido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha e obrigatoria.");
    }
}
