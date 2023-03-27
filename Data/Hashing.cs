using System.Security.Cryptography;
using System.Text;

namespace practice.Data;

public static class Hashing
{
    public static string HashPassword(string password)
    {
        var temp = Encoding.UTF8.GetBytes(password);
        var hash = SHA512.HashData(temp);
        return Convert.ToBase64String(hash);
    }
}