using FCG.Domain.Common;
using FluentAssertions;
using Xunit;

namespace FCG.Test;

public class ResultAndErrorTests
{
    [Fact]
    public void Result_Success_ShouldExposeSuccessState()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Result_Failure_ShouldExposeError()
    {
        var error = Error.InvalidRequest("Code", "Message");

        var result = Result.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ResultOfT_Value_ShouldThrow_WhenResultIsFailure()
    {
        var result = Result<string>.Failure(Errors.Users.NotFound);

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(ErrorType.Failure)]
    [InlineData(ErrorType.InvalidRequest)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Unauthorized)]
    [InlineData(ErrorType.Forbidden)]
    public void ErrorFactories_ShouldCreateExpectedErrorTypes(ErrorType expectedType)
    {
        var error = expectedType switch
        {
            ErrorType.Failure => Error.Failure("Code", "Message"),
            ErrorType.InvalidRequest => Error.InvalidRequest("Code", "Message"),
            ErrorType.Validation => Error.Validation("Code", "Message"),
            ErrorType.NotFound => Error.NotFound("Code", "Message"),
            ErrorType.Unauthorized => Error.Unauthorized("Code", "Message"),
            ErrorType.Forbidden => Error.Forbidden("Code", "Message"),
            _ => throw new ArgumentOutOfRangeException(nameof(expectedType))
        };

        error.Type.Should().Be(expectedType);
        error.Code.Should().Be("Code");
        error.Message.Should().Be("Message");
    }

    [Fact]
    public void CentralizedErrors_ShouldExposeExpectedErrors()
    {
        var errors = new[]
        {
            Errors.Games.NotFound,
            Errors.Promotions.NotFound,
            Errors.Pagination.InvalidPage,
            Errors.UnitOfWork.CommitFailed,
            Errors.Orders.EmptyGames,
            Errors.Orders.AccessDenied,
            Errors.Orders.CannotAddItemsToNonPendingOrder,
            Errors.Orders.OnlyPendingOrdersCanBeCanceled
        };

        errors.Should().OnlyContain(error => !string.IsNullOrWhiteSpace(error.Code));
        errors.Should().OnlyContain(error => !string.IsNullOrWhiteSpace(error.Message));
    }
}
