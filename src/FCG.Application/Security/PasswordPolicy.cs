using System.Text.RegularExpressions;

namespace FCG.Application.Security;

public static partial class PasswordPolicy
{
    public const int MinimumLength = 8;

    public static bool IsStrong(string password)
    {
        return !string.IsNullOrWhiteSpace(password)
            && password.Length >= MinimumLength
            && HasLetterRegex().IsMatch(password)
            && HasNumberRegex().IsMatch(password)
            && HasSpecialCharacterRegex().IsMatch(password);
    }

    [GeneratedRegex("[a-zA-Z]")]
    private static partial Regex HasLetterRegex();

    [GeneratedRegex("[0-9]")]
    private static partial Regex HasNumberRegex();

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex HasSpecialCharacterRegex();
}
