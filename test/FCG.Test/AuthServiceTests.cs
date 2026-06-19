using FCG.Application.DTOs;
using FCG.Application.Interfaces;
using FCG.Application.Security;
using FCG.Application.Services;
using FCG.Application.Settings;
using FCG.Domain.Common;
using FCG.Domain.Entities;
using FCG.Domain.Enums;
using FCG.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace FCG.Test;

public class AuthServiceTests
{
    private const string SecretKey = "test_secret";

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var passwordHash = PasswordHasher.HashPassword("Senha@123", SecretKey);
        var repository = new FakeUserRepository(new User("User", "user@email.com", passwordHash, Role.User));
        var service = CreateService(repository);

        var result = await service.LoginAsync(new LoginDto("user@email.com", "Senha@123"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("token");
    }

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenPasswordIsInvalid()
    {
        var passwordHash = PasswordHasher.HashPassword("Senha@123", SecretKey);
        var repository = new FakeUserRepository(new User("User", "user@email.com", passwordHash, Role.User));
        var service = CreateService(repository);

        var result = await service.LoginAsync(new LoginDto("user@email.com", "Errada@123"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Auth.InvalidCredentials);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUserWithUserRole_WhenEmailIsAvailable()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(repository, unitOfWork);

        var result = await service.RegisterAsync(new RegisterUserDto("User", "user@email.com", "Senha@123"));

        result.IsSuccess.Should().BeTrue();
        repository.Users.Should().ContainSingle(user => user.Email == "user@email.com" && user.Role == Role.User);
        unitOfWork.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task RegisterAsync_ShouldFail_WhenEmailAlreadyExists()
    {
        var repository = new FakeUserRepository(new User("User", "user@email.com", "hash", Role.User));
        var service = CreateService(repository);

        var result = await service.RegisterAsync(new RegisterUserDto("User", "user@email.com", "Senha@123"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Auth.RegisterFailed);
    }

    [Fact]
    public async Task RegisterAsync_ShouldFail_WhenPasswordIsWeak()
    {
        var service = CreateService(new FakeUserRepository());

        var result = await service.RegisterAsync(new RegisterUserDto("User", "user@email.com", "weak"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Auth.WeakPassword);
    }

    private static AuthService CreateService(FakeUserRepository repository, FakeUnitOfWork? unitOfWork = null)
    {
        return new AuthService(
            repository,
            new FakeTokenService(),
            unitOfWork ?? new FakeUnitOfWork(),
            Options.Create(new AuthSettings { SecretKey = SecretKey }));
    }

    private sealed class FakeTokenService : ITokenService
    {
        public string GenerateToken(User user) => "token";
    }

    private sealed class FakeUserRepository(params User[] users) : IUserRepository
    {
        public List<User> Users { get; } = users.ToList();

        public Task<Result> AddAsync(User entity)
        {
            Users.Add(entity);
            return Task.FromResult(Result.Success());
        }

        public Task<Result<PagedResult<User>>> GetAllAsync(PaginationParameters pagination) => Task.FromResult(Result<PagedResult<User>>.Success(new PagedResult<User>(Users, 1, 30, Users.Count)));

        public Task<Result<User>> GetByEmailAsync(string email)
        {
            var user = Users.FirstOrDefault(user => user.Email == email && user.IsActive);
            return Task.FromResult(user is null
                ? Result<User>.Failure(Errors.Users.NotFoundByEmail)
                : Result<User>.Success(user));
        }

        public Task<Result<User>> GetByIdAsync(Guid id)
        {
            var user = Users.FirstOrDefault(user => user.Id == id && user.IsActive);
            return Task.FromResult(user is null
                ? Result<User>.Failure(Errors.Users.NotFound)
                : Result<User>.Success(user));
        }

        public Task<Result<User>> GetByIdIncludingInactiveAsync(Guid id) => GetByIdAsync(id);
        public Task<Result<PagedResult<User>>> GetAllIncludingInactiveAsync(PaginationParameters pagination) => GetAllAsync(pagination);
        public Task<Result<User>> GetByEmailIncludingInactiveAsync(string email)
        {
            var user = Users.FirstOrDefault(user => user.Email == email);
            return Task.FromResult(user is null
                ? Result<User>.Failure(Errors.Users.NotFoundByEmail)
                : Result<User>.Success(user));
        }

        public Task<Result> UpdateAsync(User entity) => Task.FromResult(Result.Success());

        public Task<Result> DeleteAsync(User entity) => Task.FromResult(Result.Success());
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int CommitCount { get; private set; }
        public Task<Result> CommitAsync(CancellationToken cancellationToken = default)
        {
            CommitCount++;
            return Task.FromResult(Result.Success());
        }
    }
}
