using System.Text;
using FCG.API.Middlewares;
using FCG.Application.Interfaces;
using FCG.Application.Services;
using FCG.Application.Settings;
using FCG.Application.Validators;
using FCG.Domain.Interfaces;
using FCG.Infrastructure.Data;
using FCG.Infrastructure.Repositories;
using FCG.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace FCG.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiDependencies(this IServiceCollection services, ApiConfiguration configuration)
    {
        services
            .AddDatabase(configuration.ConnectionString)
            .AddRepositories()
            .AddApplicationServices(configuration)
            .AddApiAuthentication(configuration)
            .AddApiExceptionHandling()
            .AddApiDocumentation();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServerOptions => sqlServerOptions.MigrationsAssembly("FCG.Infrastructure"))
                .LogTo(Console.WriteLine, LogLevel.Information),
            ServiceLifetime.Scoped);

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services, ApiConfiguration configuration)
    {
        services.Configure<AuthSettings>(options => options.SecretKey = configuration.SecretKey);
        services.Configure<JwtSettings>(options =>
        {
            options.Key = configuration.JwtKey;
            options.Issuer = configuration.JwtIssuer;
            options.Audience = configuration.JwtAudience;
        });
        services.Configure<PaginationSettings>(options => options.PageSize = configuration.PageSize);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<IPromotionService, PromotionService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IUserService, UserService>();

        services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();

        return services;
    }

    private static IServiceCollection AddApiAuthentication(this IServiceCollection services, ApiConfiguration configuration)
    {
        var key = Encoding.ASCII.GetBytes(configuration.JwtKey);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtBearerOptions =>
            {
                jwtBearerOptions.RequireHttpsMetadata = false;
                jwtBearerOptions.SaveToken = true;
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = configuration.JwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = configuration.JwtAudience
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddApiExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    private static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(swaggerOptions =>
        {
            swaggerOptions.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT no campo abaixo comecando com 'Bearer '. Exemplo: Bearer eyJhb..."
            });

            swaggerOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
