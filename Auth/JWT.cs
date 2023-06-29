using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using practice.Models;
using System.Text;
using System.Security.Claims;

namespace practice.Auth;

public class JWT 
{
    public string GenerateJwtToken(User user)
    {
        if(user == null)
            return "";
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("G2JCIW9PkUOiN47WjTRl");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.GivenName, user.FirstName!),
            new Claim(ClaimTypes.Surname, user.LastName!),
            new Claim("CartItemsJson", user.CartItemsJson ?? ""),
            new Claim("UserId", user.Id.ToString())
        }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        System.Console.WriteLine(tokenHandler.WriteToken(token));
        return tokenHandler.WriteToken(token);
    }

    public User? GetCurrentUser(string? token)
    {
        if(token.IsNullOrEmpty())
            return null;
        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtToken = handler.ReadJwtToken(token);
        var user = new User();
        foreach (Claim claim in jwtToken.Claims)
        {
            switch (claim.Type)
            {
                case "email":
                    user.Email = claim.Value;
                    break;
                case "given_name":
                    user.FirstName = claim.Value;
                    break;
                case "family_name":
                    user.LastName = claim.Value;
                    break;
                case "CartItemsJson":
                    user.CartItemsJson = claim.Value;
                    break;
                case "UserId":
                if (int.TryParse(claim.Value, out int userId))
                    user.Id = userId;
                break;
            }
        }
        return user;
    }
}
