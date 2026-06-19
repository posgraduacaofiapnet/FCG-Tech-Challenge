using FCG.Application.DTOs;
using FCG.Application.Validators;
using FluentAssertions;
using Xunit;

namespace FCG.Test;

public class RegisterUserValidatorTests()
{
    private readonly RegisterUserValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_WhenNameIsEmpty()
    {
        // Arrange
        var model = new RegisterUserDto("", "teste@email.com", "Senh@Forte123");

        // Act
        var result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Name");
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenModelIsCorrect()
    {
        // Arrange
        var model = new RegisterUserDto("João das Neves", "joao@email.com", "Senh@Forte123");

        // Act
        var result = _validator.Validate(model);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("email-invalido")]
    [InlineData("usuario@")]
    public void Validate_ShouldHaveError_WhenEmailIsInvalid(string email)
    {
        var model = new RegisterUserDto("Joao", email, "Senha@123");

        var result = _validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Email");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("abcdefgh")]
    [InlineData("abc12345")]
    [InlineData("abcdefg!")]
    public void Validate_ShouldHaveError_WhenPasswordIsWeak(string password)
    {
        var model = new RegisterUserDto("Joao", "joao@email.com", password);

        var result = _validator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Password");
    }
}
