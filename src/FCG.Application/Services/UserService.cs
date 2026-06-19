using FCG.Application.DTOs;
using FCG.Application.Interfaces;
using FCG.Application.Security;
using FCG.Application.Settings;
using FCG.Domain.Common;
using FCG.Domain.Entities;
using FCG.Domain.Enums;
using FCG.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace FCG.Application.Services;

public sealed class UserService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IOptions<AuthSettings> authSettings,
    IOptions<PaginationSettings> paginationSettings) : IUserService
{
    private readonly int _pageSize = paginationSettings.Value.PageSize;
    private readonly string _secretKey = authSettings.Value.SecretKey;

    public async Task<Result<UserDto>> CreateAsync(CreateUserDto dto)
    {
        if (!PasswordPolicy.IsStrong(dto.Password))
        {
            return Result<UserDto>.Failure(Errors.Auth.WeakPassword);
        }

        if (!Enum.TryParse<Role>(dto.Role, ignoreCase: true, out var role))
        {
            return Result<UserDto>.Failure(Errors.Users.InvalidRole);
        }

        var existingUser = await userRepository.GetByEmailIncludingInactiveAsync(dto.Email);
        if (existingUser.IsSuccess)
        {
            return Result<UserDto>.Failure(Errors.Users.EmailAlreadyRegistered);
        }

        var passwordHash = PasswordHasher.HashPassword(dto.Password, _secretKey);
        var user = new User(dto.Name, dto.Email, passwordHash, role);

        var addResult = await userRepository.AddAsync(user);
        if (addResult.IsFailure)
        {
            return Result<UserDto>.Failure(addResult.Error!);
        }

        var commitResult = await unitOfWork.CommitAsync();
        return commitResult.IsFailure
            ? Result<UserDto>.Failure(commitResult.Error!)
            : Result<UserDto>.Success(MapToDto(user));
    }

    public async Task<Result<PagedResult<UserDto>>> GetAllAsync(int page)
    {
        if (page < 1)
        {
            return Result<PagedResult<UserDto>>.Failure(Errors.Pagination.InvalidPage);
        }

        var usersResult = await userRepository.GetAllIncludingInactiveAsync(new PaginationParameters(page, _pageSize));
        if (usersResult.IsFailure)
        {
            return Result<PagedResult<UserDto>>.Failure(usersResult.Error!);
        }

        var users = usersResult.Value;
        return Result<PagedResult<UserDto>>.Success(new PagedResult<UserDto>(
            users.Items.Select(MapToDto).ToArray(),
            users.Page,
            users.PageSize,
            users.TotalCount));
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid id)
    {
        var userResult = await userRepository.GetByIdIncludingInactiveAsync(id);
        return userResult.IsFailure
            ? Result<UserDto>.Failure(userResult.Error!)
            : Result<UserDto>.Success(MapToDto(userResult.Value));
    }

    public async Task<Result<UserDto>> UpdateRoleAsync(Guid id, UpdateUserRoleDto dto)
    {
        if (!Enum.TryParse<Role>(dto.Role, ignoreCase: true, out var role))
        {
            return Result<UserDto>.Failure(Errors.Users.InvalidRole);
        }

        var userResult = await userRepository.GetByIdIncludingInactiveAsync(id);
        if (userResult.IsFailure)
        {
            return Result<UserDto>.Failure(userResult.Error!);
        }

        userResult.Value.ChangeRole(role);

        var updateResult = await userRepository.UpdateAsync(userResult.Value);
        if (updateResult.IsFailure)
        {
            return Result<UserDto>.Failure(updateResult.Error!);
        }

        var commitResult = await unitOfWork.CommitAsync();
        return commitResult.IsFailure
            ? Result<UserDto>.Failure(commitResult.Error!)
            : Result<UserDto>.Success(MapToDto(userResult.Value));
    }

    public async Task<Result<UserDto>> DeactivateAsync(Guid id)
    {
        var userResult = await userRepository.GetByIdIncludingInactiveAsync(id);
        if (userResult.IsFailure)
        {
            return Result<UserDto>.Failure(userResult.Error!);
        }

        if (string.Equals(userResult.Value.Email, SystemUsers.DefaultAdminEmail, StringComparison.OrdinalIgnoreCase))
        {
            return Result<UserDto>.Failure(Errors.Users.DefaultAdminCannotBeDeleted);
        }

        userResult.Value.Deactivate();

        var updateResult = await userRepository.UpdateAsync(userResult.Value);
        if (updateResult.IsFailure)
        {
            return Result<UserDto>.Failure(updateResult.Error!);
        }

        var commitResult = await unitOfWork.CommitAsync();
        return commitResult.IsFailure
            ? Result<UserDto>.Failure(commitResult.Error!)
            : Result<UserDto>.Success(MapToDto(userResult.Value));
    }

    public async Task<Result<UserDto>> ReactivateAsync(Guid id)
    {
        var userResult = await userRepository.GetByIdIncludingInactiveAsync(id);
        if (userResult.IsFailure)
        {
            return Result<UserDto>.Failure(userResult.Error!);
        }

        userResult.Value.Reactivate();

        var updateResult = await userRepository.UpdateAsync(userResult.Value);
        if (updateResult.IsFailure)
        {
            return Result<UserDto>.Failure(updateResult.Error!);
        }

        var commitResult = await unitOfWork.CommitAsync();
        return commitResult.IsFailure
            ? Result<UserDto>.Failure(commitResult.Error!)
            : Result<UserDto>.Success(MapToDto(userResult.Value));
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Name,
            user.Email,
            user.Role.ToString(),
            user.IsActive,
            user.DeactivatedAt,
            user.ReactivatedAt);
    }
}
