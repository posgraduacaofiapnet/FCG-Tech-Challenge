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

public sealed class AuthService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IUnitOfWork unitOfWork,
    IOptions<AuthSettings> authSettings) : IAuthService
{
    private readonly string _secretKey = authSettings.Value.SecretKey;

    public async Task<Result<TokenDto>> LoginAsync(LoginDto dto)
    {
        var userResult = await userRepository.GetByEmailAsync(dto.Email);
        if (userResult.IsFailure || !PasswordHasher.VerifyPassword(dto.Password, userResult.Value.PasswordHash, _secretKey))
        {
            return Result<TokenDto>.Failure(Errors.Auth.InvalidCredentials);
        }

        return Result<TokenDto>.Success(new TokenDto(tokenService.GenerateToken(userResult.Value)));
    }

    public async Task<Result> RegisterAsync(RegisterUserDto dto)
    {
        if (!PasswordPolicy.IsStrong(dto.Password))
        {
            return Result.Failure(Errors.Auth.WeakPassword);
        }

        var existingUser = await userRepository.GetByEmailAsync(dto.Email);
        if (existingUser.IsSuccess)
        {
            return Result.Failure(Errors.Auth.RegisterFailed);
        }

        var passHash = PasswordHasher.HashPassword(dto.Password, _secretKey);
        var user = new User(dto.Name, dto.Email, passHash, Role.User);

        var addResult = await userRepository.AddAsync(user);
        return addResult.IsFailure ? addResult : await unitOfWork.CommitAsync();
    }

}
