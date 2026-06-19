using FCG.Application.Security;
using FluentAssertions;
using Xunit;

namespace FCG.Test;

public class PasswordPolicyTests
{
    [Theory]
    [InlineData("Abc!1234")]
    [InlineData("senha@123")]
    public void IsStrong_ShouldReturnTrue_WhenPasswordMeetsRequiredRules(string password)
    {
        PasswordPolicy.IsStrong(password).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("abcdefgh")]
    [InlineData("abc12345")]
    [InlineData("abcdefg!")]
    public void IsStrong_ShouldReturnFalse_WhenPasswordDoesNotMeetRequiredRules(string password)
    {
        PasswordPolicy.IsStrong(password).Should().BeFalse();
    }
}
