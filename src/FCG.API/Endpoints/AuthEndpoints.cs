using FCG.API.Extensions;
using FCG.Application.DTOs;
using FCG.Application.Interfaces;
using FluentValidation;

namespace FCG.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Autenticacao");

        group.MapPost("/register", async (RegisterUserDto dto, IAuthService service, IValidator<RegisterUserDto> validator) =>
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                return validation.ToBadRequestValidationProblem();
            }

            var result = await service.RegisterAsync(dto);
            return result.ToHttpResult(() => Results.Created("/api/auth/register", new { message = "Usuario registrado com sucesso." }));
        });

        group.MapPost("/login", async (LoginDto dto, IAuthService service, IValidator<LoginDto> validator) =>
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                return validation.ToBadRequestValidationProblem();
            }

            var result = await service.LoginAsync(dto);
            return result.ToHttpResult(Results.Ok);
        });
    }
}
