using Sneakers.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Sneakers.Interfaces;
using Microsoft.AspNetCore.Authentication;

namespace Sneakers.Auth;

public class Auth
{
    public async Task SignIn(User user, IUserRepository _repository, HttpContext httpContext)
    {
        user = await _repository.GetByEmailAsync(user.Email!) ?? new User();

        var claimsIdentity = new ClaimsIdentity(new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FirstName!),
            new Claim(ClaimTypes.Surname, user.LastName!),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim("CartItemsJson", user.CartItemsJson ?? "")

            }, CookieAuthenticationDefaults.AuthenticationScheme);


        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
    }

    public User GetCurrentUser(HttpContext httpContext)
    {
        var claimsPrincipal = httpContext.User;
        string? id = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        string? firstName = claimsPrincipal.FindFirstValue(ClaimTypes.Name);
        string? lastName = claimsPrincipal.FindFirstValue(ClaimTypes.Surname);
        string? email = claimsPrincipal.FindFirstValue(ClaimTypes.Email);
        string? cartItemsJson = claimsPrincipal.FindFirstValue("CartItemsJson");

        return new User
        {
            Id = Convert.ToInt32(id ?? null),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            CartItemsJson = cartItemsJson
        };
    }
}
