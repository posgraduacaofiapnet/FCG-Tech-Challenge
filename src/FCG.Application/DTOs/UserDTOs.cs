namespace FCG.Application.DTOs;

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    string Role,
    bool IsActive,
    DateTime? DeactivatedAt,
    DateTime? ReactivatedAt);

public record CreateUserDto(string Name, string Email, string Password, string Role);

public record UpdateUserRoleDto(string Role);
