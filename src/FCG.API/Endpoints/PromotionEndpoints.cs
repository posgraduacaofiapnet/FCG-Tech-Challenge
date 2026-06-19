using FCG.API.Extensions;
using FCG.Application.DTOs;
using FCG.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.API.Endpoints;

public static class PromotionEndpoints
{
    public static void MapPromotionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/promotions").WithTags("Promocoes").RequireAuthorization();

        group.MapPost("/", [Authorize(Roles = "Admin")] async (CreatePromotionDto dto, IPromotionService service, IValidator<CreatePromotionDto> validator) =>
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                return validation.ToBadRequestValidationProblem();
            }

            var result = await service.CreateAsync(dto);
            return result.ToHttpResult(promotion => Results.Created($"/api/promotions/{promotion.Id}", promotion));
        });

        group.MapGet("/", async (IPromotionService service, [FromQuery] int page = 1) =>
        {
            var result = await service.GetAllActiveAsync(page);
            return result.ToHttpResult(Results.Ok);
        }).AllowAnonymous();

        group.MapDelete("/{id:guid}", [Authorize(Roles = "Admin")] async (Guid id, IPromotionService service) =>
        {
            var result = await service.DeactivateAsync(id);
            return result.ToHttpResult(() => Results.Ok(new { message = "Promocao desativada com sucesso." }));
        });
    }
}
