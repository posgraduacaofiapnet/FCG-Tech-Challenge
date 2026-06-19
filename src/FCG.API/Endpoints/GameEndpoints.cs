using FCG.API.Extensions;
using FCG.Application.DTOs;
using FCG.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FCG.API.Endpoints;

public static class GameEndpoints
{
    public static void MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/games").WithTags("Jogos (Catalogo)").RequireAuthorization();

        group.MapPost("/", [Authorize(Roles = "Admin")] async (CreateGameDto dto, IGameService service, IValidator<CreateGameDto> validator) =>
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                return validation.ToBadRequestValidationProblem();
            }

            var result = await service.CreateAsync(dto);
            return result.ToHttpResult(game => Results.Created($"/api/games/{game.Id}", game));
        });

        group.MapGet("/", async (IGameService service, [FromQuery] int page = 1) =>
        {
            var result = await service.GetAllAsync(page);
            return result.ToHttpResult(Results.Ok);
        }).AllowAnonymous();

        group.MapGet("/{id:guid}", async (Guid id, IGameService service) =>
        {
            var result = await service.GetByIdAsync(id);
            return result.ToHttpResult(Results.Ok);
        }).AllowAnonymous();

        var library = app.MapGroup("/api/library").WithTags("Biblioteca do Usuario").RequireAuthorization();

        library.MapGet("/", async (IGameService service, ClaimsPrincipal user) =>
        {
            if (!user.TryGetUserId(out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await service.GetLibraryAsync(userId);
            return result.ToHttpResult(Results.Ok);
        });
    }
}
