using FCG.Application.DTOs;
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

public class UserServiceTests
{
    [Fact]
    public async Task UpdateRoleAsync_ShouldReturnInvalidRole_WhenRoleDoesNotExist()
    {
        var service = CreateService(new FakeUserRepository());

        var result = await service.UpdateRoleAsync(Guid.NewGuid(), new UpdateUserRoleDto("Owner"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Users.InvalidRole);
    }

    [Fact]
    public async Task UpdateRoleAsync_ShouldChangeUserRole_WhenRoleIsValid()
    {
        var user = new User("Joao", "joao@email.com", "hash", Role.User);
        var repository = new FakeUserRepository(user);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(repository, unitOfWork);

        var result = await service.UpdateRoleAsync(user.Id, new UpdateUserRoleDto("Admin"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be(Role.Admin.ToString());
        user.Role.Should().Be(Role.Admin);
        unitOfWork.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedUsers()
    {
        var repository = new FakeUserRepository(
            new User("Ana", "ana@email.com", "hash", Role.User),
            new User("Admin", "admin@email.com", "hash", Role.Admin));
        var service = CreateService(repository);

        var result = await service.GetAllAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.PageSize.Should().Be(30);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        var user = new User("Ana", "ana@email.com", "hash", Role.User);
        var service = CreateService(new FakeUserRepository(user));

        var result = await service.GetByIdAsync(user.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAdminUser_WhenRoleIsAdmin()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(repository, unitOfWork);

        var result = await service.CreateAsync(new CreateUserDto("Admin 2", "admin2@email.com", "Adm!n123", "Admin"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be(Role.Admin.ToString());
        repository.Users.Should().ContainSingle(user => user.Email == "admin2@email.com" && user.Role == Role.Admin);
        unitOfWork.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenEmailAlreadyExists()
    {
        var repository = new FakeUserRepository(new User("Admin 2", "admin2@email.com", "hash", Role.Admin));
        var service = CreateService(repository);

        var result = await service.CreateAsync(new CreateUserDto("Admin 2", "admin2@email.com", "Adm!n123", "Admin"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Users.EmailAlreadyRegistered);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenPasswordIsWeak()
    {
        var service = CreateService(new FakeUserRepository());

        var result = await service.CreateAsync(new CreateUserDto("Admin 2", "admin2@email.com", "weak", "Admin"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Auth.WeakPassword);
    }

    [Fact]
    public async Task DeleteAsync_ShouldFail_WhenUserIsDefaultAdmin()
    {
        var defaultAdmin = new User("FCG Admin", SystemUsers.DefaultAdminEmail, "hash", Role.Admin);
        var repository = new FakeUserRepository(defaultAdmin);
        var service = CreateService(repository);

        var result = await service.DeactivateAsync(defaultAdmin.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Errors.Users.DefaultAdminCannotBeDeleted);
        defaultAdmin.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateAsync_ShouldInactivateUser_WhenUserIsNotDefaultAdmin()
    {
        var user = new User("User", "user@email.com", "hash", Role.User);
        var repository = new FakeUserRepository(user);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(repository, unitOfWork);

        var result = await service.DeactivateAsync(user.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsActive.Should().BeFalse();
        result.Value.DeactivatedAt.Should().NotBeNull();
        user.IsActive.Should().BeFalse();
        user.DeactivatedAt.Should().NotBeNull();
        unitOfWork.CommitCount.Should().Be(1);
    }

    [Fact]
    public async Task ReactivateAsync_ShouldReactivateUser_KeepingDeactivationDateAsHistory()
    {
        var user = new User("User", "user@email.com", "hash", Role.User);
        user.Deactivate();
        var deactivatedAt = user.DeactivatedAt;
        var repository = new FakeUserRepository(user);
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(repository, unitOfWork);

        var result = await service.ReactivateAsync(user.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsActive.Should().BeTrue();
        result.Value.DeactivatedAt.Should().Be(deactivatedAt);
        result.Value.ReactivatedAt.Should().NotBeNull();
        unitOfWork.CommitCount.Should().Be(1);
    }

    private static UserService CreateService(FakeUserRepository repository, FakeUnitOfWork? unitOfWork = null)
    {
        return new UserService(
            repository,
            unitOfWork ?? new FakeUnitOfWork(),
            Options.Create(new AuthSettings { SecretKey = "test_secret" }),
            Options.Create(new PaginationSettings { PageSize = 30 }));
    }

    private sealed class FakeUserRepository(params User[] users) : IUserRepository
    {
        public List<User> Users { get; } = users.ToList();

        public Task<Result> AddAsync(User entity)
        {
            Users.Add(entity);
            return Task.FromResult(Result.Success());
        }

        public Task<Result<PagedResult<User>>> GetAllAsync(PaginationParameters pagination)
        {
            var items = Users
                .Skip(pagination.Skip)
                .Take(pagination.PageSize)
                .ToArray();

            return Task.FromResult(Result<PagedResult<User>>.Success(
                new PagedResult<User>(items, pagination.Page, pagination.PageSize, Users.Count)));
        }

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

        public Task<Result<User>> GetByIdIncludingInactiveAsync(Guid id)
        {
            var user = Users.FirstOrDefault(user => user.Id == id);
            return Task.FromResult(user is null
                ? Result<User>.Failure(Errors.Users.NotFound)
                : Result<User>.Success(user));
        }

        public Task<Result<PagedResult<User>>> GetAllIncludingInactiveAsync(PaginationParameters pagination)
        {
            var items = Users
                .Skip(pagination.Skip)
                .Take(pagination.PageSize)
                .ToArray();

            return Task.FromResult(Result<PagedResult<User>>.Success(
                new PagedResult<User>(items, pagination.Page, pagination.PageSize, Users.Count)));
        }

        public Task<Result<User>> GetByEmailIncludingInactiveAsync(string email)
        {
            var user = Users.FirstOrDefault(user => user.Email == email);
            return Task.FromResult(user is null
                ? Result<User>.Failure(Errors.Users.NotFoundByEmail)
                : Result<User>.Success(user));
        }

        public Task<Result> UpdateAsync(User entity)
        {
            return Task.FromResult(Result.Success());
        }

        public Task<Result> DeleteAsync(User entity)
        {
            Users.Remove(entity);
            return Task.FromResult(Result.Success());
        }
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
