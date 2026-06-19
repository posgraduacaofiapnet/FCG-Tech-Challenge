namespace FCG.API.Extensions;

public sealed record ApiConfiguration(
    string ConnectionString,
    string SecretKey,
    string AdminPassword,
    string JwtKey,
    string JwtIssuer,
    string JwtAudience,
    int PageSize);

public static class ConfigurationExtensions
{
    public static ApiConfiguration GetApiConfiguration(this IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("A configuracao 'ConnectionStrings:DefaultConnection' e obrigatoria.");

        var secretKey = configuration.GetRequiredValue(
            "chave_secreta",
            "CHAVE_SECRETA",
            "Auth__SecretKey");

        var adminPassword = configuration.GetOptionalValue(
            "Admin_Password",
            "ADMIN_PASSWORD") ?? "Adm!n123";

        var jwtKey = configuration.GetRequiredValue(
            "Jwt_Key",
            "JWT_KEY",
            "Jwt__Key");

        var jwtIssuer = configuration.GetOptionalValue("Jwt_Issuer", "JWT_ISSUER", "Jwt__Issuer") ?? "FCG.API";
        var jwtAudience = configuration.GetOptionalValue("Jwt_Audience", "JWT_AUDIENCE", "Jwt__Audience") ?? "FCG.API";

        var pageSize = configuration.GetValue<int?>("Pagination:PageSize") ?? 30;
        if (pageSize < 1)
        {
            throw new InvalidOperationException("A configuracao 'Pagination:PageSize' deve ser maior ou igual a 1.");
        }

        return new ApiConfiguration(connectionString, secretKey, adminPassword, jwtKey, jwtIssuer, jwtAudience, pageSize);
    }

    private static string GetRequiredValue(this IConfiguration configuration, params string[] keys)
    {
        return configuration.GetOptionalValue(keys)
            ?? throw new InvalidOperationException($"A configuracao '{keys[0]}' e obrigatoria.");
    }

    private static string? GetOptionalValue(this IConfiguration configuration, params string[] keys)
    {
        foreach (var key in keys)
        {
            var value = configuration[key];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}
