using FCG.Application.DTOs;
using FCG.Application.Security;
using FluentValidation;

namespace FCG.Application.Validators;

public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserValidator()
    {
        RuleFor(user => user.Name).NotEmpty().WithMessage("Nome e obrigatorio.");

        RuleFor(user => user.Email).NotEmpty().EmailAddress().WithMessage("E-mail invalido.");

        RuleFor(user => user.Password)
            .NotEmpty().WithMessage("Senha e obrigatoria.")
            .MinimumLength(PasswordPolicy.MinimumLength).WithMessage("Senha deve ter no minimo 8 caracteres.")
            .Matches("[a-zA-Z]").WithMessage("A senha deve conter pelo menos uma letra.")
            .Matches("[0-9]").WithMessage("A senha deve conter pelo menos um numero.")
            .Matches("[^a-zA-Z0-9]").WithMessage("A senha deve conter pelo menos um caractere especial.");
    }
}
