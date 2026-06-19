using System.Security.Claims;
using FCG.Domain.Enums;

namespace FCG.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool TryGetUserId(this ClaimsPrincipal user, out Guid userId)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("user_id");

        return Guid.TryParse(userIdClaim, out userId);
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole(Role.Admin.ToString())
            || string.Equals(user.FindFirstValue("role"), Role.Admin.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
