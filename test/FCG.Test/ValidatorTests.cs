using FCG.Application.DTOs;
using FCG.Application.Validators;
using FluentAssertions;
using Xunit;

namespace FCG.Test;

public class ValidatorTests
{
    [Fact]
    public void LoginValidator_ShouldValidateEmailAndPassword()
    {
        var validator = new LoginValidator();

        var result = validator.Validate(new LoginDto("invalid", ""));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Email");
        result.Errors.Should().Contain(error => error.PropertyName == "Password");
    }

    [Fact]
    public void CreateGameValidator_ShouldValidateRequiredFields()
    {
        var validator = new CreateGameValidator();

        var result = validator.Validate(new CreateGameDto("", "", 0));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Title");
        result.Errors.Should().Contain(error => error.PropertyName == "Description");
        result.Errors.Should().Contain(error => error.PropertyName == "Price");
    }

    [Fact]
    public void CreateOrderValidator_ShouldValidateGameIds()
    {
        var validator = new CreateOrderValidator();

        var result = validator.Validate(new CreateOrderDto([]));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "GameIds");
    }

    [Fact]
    public void CreatePromotionValidator_ShouldValidatePromotionFields()
    {
        var validator = new CreatePromotionValidator();

        var result = validator.Validate(new CreatePromotionDto("", Guid.Empty, 0, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1)));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Name");
        result.Errors.Should().Contain(error => error.PropertyName == "GameId");
        result.Errors.Should().Contain(error => error.PropertyName == "DiscountPercentage");
        result.Errors.Should().Contain(error => error.PropertyName == "EndDate");
    }

    [Fact]
    public void UpdateUserRoleValidator_ShouldValidateRole()
    {
        var validator = new UpdateUserRoleValidator();

        var result = validator.Validate(new UpdateUserRoleDto("Owner"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Role");
    }

    [Fact]
    public void CreateUserValidator_ShouldValidateAdminUserCreation()
    {
        var validator = new CreateUserValidator();

        var result = validator.Validate(new CreateUserDto("", "invalid", "weak", "Owner"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Name");
        result.Errors.Should().Contain(error => error.PropertyName == "Email");
        result.Errors.Should().Contain(error => error.PropertyName == "Password");
        result.Errors.Should().Contain(error => error.PropertyName == "Role");
    }

    [Fact]
    public void CreateUserValidator_ShouldBeValid_WhenAdminUserIsValid()
    {
        var validator = new CreateUserValidator();

        var result = validator.Validate(new CreateUserDto("Admin", "admin@email.com", "Adm!n123", "Admin"));

        result.IsValid.Should().BeTrue();
    }
}
