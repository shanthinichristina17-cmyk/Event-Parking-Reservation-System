using System.Security.Cryptography;
using System.Text;

namespace EventParkingSystem.API.Common;

public static class SecureTokenGenerator
{
    public static string GenerateRawToken(int bytes = 32) => Convert.ToHexString(RandomNumberGenerator.GetBytes(bytes));

    public static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
