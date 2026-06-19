using FCG.API.Extensions;
using FCG.Application.DTOs;
using FCG.Application.Interfaces;
using FluentValidation;
using System.Security.Claims;

namespace FCG.API.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Pedidos").RequireAuthorization();

        group.MapPost("/", async (CreateOrderDto dto, IOrderService service, ClaimsPrincipal user, IValidator<CreateOrderDto> validator) =>
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                return validation.ToBadRequestValidationProblem();
            }

            if (!user.TryGetUserId(out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await service.CreateOrderAsync(userId, dto);
            return result.ToHttpResult(order => Results.Created($"/api/orders/{order.Id}", order));
        });

        group.MapGet("/{id:guid}", async (Guid id, IOrderService service, ClaimsPrincipal user) =>
        {
            if (!user.TryGetUserId(out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await service.GetOrderByIdAsync(id, userId, user.IsAdmin());
            return result.ToHttpResult(Results.Ok);
        });

        group.MapGet("/", async (IOrderService service, ClaimsPrincipal user) =>
        {
            if (!user.TryGetUserId(out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await service.GetUserOrdersAsync(userId);
            return result.ToHttpResult(Results.Ok);
        });

        group.MapPost("/{id:guid}/pay", async (Guid id, IOrderService service, ClaimsPrincipal user) =>
        {
            if (!user.TryGetUserId(out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await service.ApprovePaymentAsync(id, userId, user.IsAdmin());
            return result.ToHttpResult(() => Results.Ok(new { message = "Pagamento aprovado e jogos adicionados a biblioteca." }));
        });
    }
}
