using FCG.Application.DTOs;
using FluentValidation;

namespace FCG.Application.Validators;

public class CreatePromotionValidator : AbstractValidator<CreatePromotionDto>
{
    public CreatePromotionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome da promoção é obrigatório.")
            .MaximumLength(150).WithMessage("O nome da promoção não pode exceder 150 caracteres.");

        RuleFor(x => x.GameId)
            .NotEmpty().WithMessage("O ID do jogo é obrigatório.");

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(1, 100).WithMessage("O percentual de desconto deve estar entre 1 e 100.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("A data de início é obrigatória.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("A data de término é obrigatória.")
            .GreaterThan(x => x.StartDate).WithMessage("A data de término deve ser após a data de início.");
    }
}
