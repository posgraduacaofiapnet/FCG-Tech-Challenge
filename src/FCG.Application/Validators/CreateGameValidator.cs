using FCG.Application.DTOs;
using FluentValidation;

namespace FCG.Application.Validators;

public class CreateGameValidator : AbstractValidator<CreateGameDto>
{
    public CreateGameValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Titulo e obrigatorio.")
            .MaximumLength(150).WithMessage("Titulo nao pode exceder 150 caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descricao e obrigatoria.")
            .MaximumLength(1000).WithMessage("Descricao nao pode exceder 1000 caracteres.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Preco deve ser maior que zero.");
    }
}
