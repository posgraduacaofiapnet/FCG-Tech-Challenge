using FluentValidation.Results;

namespace FCG.API.Extensions;

public static class ValidationResultExtensions
{
    public static IResult ToBadRequestValidationProblem(this ValidationResult validation)
    {
        return Results.ValidationProblem(validation.ToErrors(), statusCode: StatusCodes.Status400BadRequest);
    }

    public static Dictionary<string, string[]> ToErrors(this ValidationResult validation)
    {
        return validation.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray()
            );
    }
}
