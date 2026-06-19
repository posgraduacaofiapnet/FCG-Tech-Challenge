using FCG.Application.DTOs;
using FluentValidation;

namespace FCG.Application.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.GameIds)
            .NotEmpty().WithMessage("Informe ao menos um jogo para criar o pedido.");

        RuleForEach(x => x.GameIds)
            .NotEmpty().WithMessage("ID do jogo e obrigatorio.");
    }
}
