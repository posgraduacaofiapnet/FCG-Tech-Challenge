using FCG.Application.Security;
using FCG.Domain.Common;
using FCG.Domain.Entities;
using FCG.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Data;

public static class DatabaseSeeder
{
    private const string AdminName = "FCG Admin";

    public static async Task SeedAsync(AppDbContext context, string secretKey, string adminPassword)
    {
        if (!PasswordPolicy.IsStrong(adminPassword))
        {
            throw new InvalidOperationException("A senha do administrador seedado deve ter no minimo 8 caracteres, letras, numeros e caracteres especiais.");
        }

        var adminExists = await context.Users.AnyAsync(user => user.Email == SystemUsers.DefaultAdminEmail);
        if (adminExists)
        {
            return;
        }

        var passwordHash = PasswordHasher.HashPassword(adminPassword, secretKey);
        var admin = new User(AdminName, SystemUsers.DefaultAdminEmail, passwordHash, Role.Admin);

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();
    }
}
