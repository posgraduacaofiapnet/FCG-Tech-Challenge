using Isopoh.Cryptography.Argon2;

namespace FCG.Application.Security;

public static class PasswordHasher
{
    private const int Argon2MemoryCost = 65536;
    private const int Argon2Iterations = 3;
    private const int Argon2Threads = 4;
    private const int Argon2HashLength = 32;

    public static string HashPassword(string password, string secretKey)
    {
        return Argon2.Hash(
            ConcatPasswordWithSecret(password, secretKey),
            timeCost: Argon2Iterations,
            memoryCost: Argon2MemoryCost,
            parallelism: Argon2Threads,
            type: Argon2Type.HybridAddressing,
            hashLength: Argon2HashLength);
    }

    public static bool VerifyPassword(string password, string passwordHash, string secretKey)
    {
        return Argon2.Verify(passwordHash, ConcatPasswordWithSecret(password, secretKey));
    }

    private static string ConcatPasswordWithSecret(string password, string secretKey)
    {
        return $"{password}{secretKey}";
    }
}
