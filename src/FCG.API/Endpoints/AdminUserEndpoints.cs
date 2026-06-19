using FCG.API.Extensions;
using FCG.Application.DTOs;
using FCG.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.API.Endpoints;

public static class AdminUserEndpoints
{
    public static void MapAdminUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/users")
            .WithTags("Administracao de Usuarios")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

        group.MapGet("/", async (IUserService service, [FromQuery] int page = 1) =>
        {
            var result = await service.GetAllAsync(page);
            return result.ToHttpResult(Results.Ok);
        });

        group.MapGet("/{id:guid}", async (Guid id, IUserService service) =>
        {
            var result = await service.GetByIdAsync(id);
            return result.ToHttpResult(Results.Ok);
        });

        group.MapPost("/", async (CreateUserDto dto, IUserService service, IValidator<CreateUserDto> validator) =>
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                return validation.ToBadRequestValidationProblem();
            }

            var result = await service.CreateAsync(dto);
            return result.ToHttpResult(user => Results.Created($"/api/admin/users/{user.Id}", user));
        });

        group.MapPatch("/{id:guid}/role", async (Guid id, UpdateUserRoleDto dto, IUserService service, IValidator<UpdateUserRoleDto> validator) =>
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                return validation.ToBadRequestValidationProblem();
            }

            var result = await service.UpdateRoleAsync(id, dto);
            return result.ToHttpResult(Results.Ok);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IUserService service) =>
        {
            var result = await service.DeactivateAsync(id);
            return result.ToHttpResult(Results.Ok);
        });

        group.MapPatch("/{id:guid}/reactivate", async (Guid id, IUserService service) =>
        {
            var result = await service.ReactivateAsync(id);
            return result.ToHttpResult(Results.Ok);
        });
    }
}
