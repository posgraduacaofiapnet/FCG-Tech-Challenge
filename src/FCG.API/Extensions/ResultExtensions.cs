using FCG.Domain.Common;

namespace FCG.API.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result, Func<IResult> onSuccess)
    {
        return result.IsSuccess ? onSuccess() : ToProblem(result.Error!);
    }

    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        return result.IsSuccess ? onSuccess(result.Value) : ToProblem(result.Error!);
    }

    private static IResult ToProblem(Error error)
    {
        return error.Type switch
        {
            ErrorType.InvalidRequest => Results.BadRequest(new { error.Code, error.Message }),
            ErrorType.Validation => Results.UnprocessableEntity(new { error.Code, error.Message }),
            ErrorType.NotFound => Results.NotFound(new { error.Code, error.Message }),
            ErrorType.Unauthorized => Results.Unauthorized(),
            ErrorType.Forbidden => Results.Forbid(),
            _ => Results.Problem(error.Message)
        };
    }
}
