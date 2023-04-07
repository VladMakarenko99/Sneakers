using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;
using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using practice.Data;
using practice.Models;
using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace practice.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context) => this._context = context;


    [Route("/account/register")]
    [HttpGet]
    public IActionResult Register()
    {
        if (HttpContext.Session.GetString("userEmail") != null)
            return NotFound();


        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(User user)
    {
        string[] emailFormats = new string[] { "@yahoo.com", "@gmail.com", "@outlook.com", "@hotmail.com", "@ukr.net" };
        if (string.IsNullOrEmpty(user.FirstName))
            ModelState.AddModelError(nameof(user.FirstName), "This field is required!");

        if (string.IsNullOrEmpty(user.LastName))
            ModelState.AddModelError(nameof(user.LastName), "This field is required!");

        if (string.IsNullOrEmpty(user.Email))
            ModelState.AddModelError(nameof(user.Email), "This field is required!");

        if (string.IsNullOrEmpty(user.Password))
            ModelState.AddModelError(nameof(user.Password), "This field is required!");

        if (string.IsNullOrEmpty(user.ConfirmedPass))
            ModelState.AddModelError(nameof(user.ConfirmedPass), "This field is required!");

        if (!string.IsNullOrEmpty(user.Email) && !Regex.IsMatch(user.Email, @"\w+@\w+\.\w+"))
            ModelState.AddModelError(nameof(user.Email), "The email format is incorrect!");

        if (!string.IsNullOrEmpty(user.Email) && _context.Users.FirstOrDefault(x => x.Email == user.Email) != null)
            ModelState.AddModelError(nameof(user.Email), "This email is already registered!");

        if (!string.IsNullOrEmpty(user.FirstName) && Convert.ToString(user.FirstName).Length > 20)
            ModelState.AddModelError(nameof(user.FirstName), "First name cant be over then 20 characters!");

        if (!string.IsNullOrEmpty(user.LastName) && Convert.ToString(user.LastName).Length > 20)
            ModelState.AddModelError(nameof(user.FirstName), "Last name cant be over then 20 characters!");

        if (!string.IsNullOrEmpty(user.Password) && Convert.ToString(user.Password).Length > 20)
            ModelState.AddModelError(nameof(user.Password), "Password cant be over then 20 characters!");

        if (!string.IsNullOrEmpty(user.Password) && Convert.ToString(user.Password).Length < 5)
            ModelState.AddModelError(nameof(user.Password), "Password cant be less then 5 characters!");

        if (Convert.ToString(user.Password) != Convert.ToString(user.ConfirmedPass))
            ModelState.AddModelError(nameof(user.Password), "Passwords are different!");

        if (!ModelState.IsValid) return View(user);

        var salt = DateTime.Now.ToString(CultureInfo.CurrentCulture);
        user = new User(user.FirstName ?? "", user.LastName ?? "", user.Email ?? "", Hashing.HashPassword($"{user.Password!}{salt}"), Hashing.HashPassword($"{user.ConfirmedPass!}{salt}"), salt);
        await _context.AddAsync(user);
        await _context.SaveChangesAsync();

        HttpContext.Session.SetString("userFirstName", user.FirstName ?? "");
        HttpContext.Session.SetString("userLastName", user.LastName ?? "");
        HttpContext.Session.SetString("userEmail", user.Email ?? "");



        return Redirect("/");
    }

    [Route("/account/login")]
    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("userEmail") != null)
            return NotFound();
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginModel model)
    {
        if (string.IsNullOrEmpty(model.Email))
            ModelState.AddModelError(nameof(model.Email), "This field is required!");

        if (string.IsNullOrEmpty(model.Password))
            ModelState.AddModelError(nameof(model.Password), "This field is required!");

        if (!ModelState.IsValid) return View("Login", model);


        var userFound = _context.Users.FirstOrDefault(x => x.Email == model.Email);

        if (Hashing.HashPassword($"{model.Password}{userFound?.Salt}") == userFound?.Password)
        {

            HttpContext.Session.SetString("userFirstName", userFound.FirstName ?? "");
            HttpContext.Session.SetString("userLastName", userFound.LastName ?? "");
            HttpContext.Session.SetString("userEmail", userFound.Email ?? "");

            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("81efc4e3f15367", "57ba73d15a23d3"),
                EnableSsl = true
            };

            client.Send("i.e 3aeccdb09f-0167be+1@inbox.mailtrap.io", Convert.ToString(userFound.Email)!,
                $@"Login is successful, {userFound.FirstName} {userFound.LastName}!", "");

            return Redirect("/");
        }

        ModelState.AddModelError(nameof(model.Email), "Incorrect email or password!");

        ModelState.AddModelError(nameof(model.Password), "Incorrect login or password!");

        return View("Login", model);
    }

    public RedirectResult Logout()
    {
        HttpContext.Session.Remove("userFirstName");
        HttpContext.Session.Remove("userLastName");
        HttpContext.Session.Remove("userEmail");

        return Redirect("/");
    }

    [Route("/profile/{email}")]
    [HttpGet]
    public IActionResult Profile(string email)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("userEmail")))
            return NotFound();
        return View();
    }

    public async Task<IActionResult> DeleteAccount()
    {
        string? email = HttpContext.Session.GetString("userEmail");
        if (string.IsNullOrEmpty(email))
            return NotFound();
        var userFound = _context.Users.FirstOrDefault(x => x.Email == email) ?? new User();
        _context.Users.Remove(userFound);
        await _context.SaveChangesAsync();
        HttpContext.Session.Remove("userFirstName");
        HttpContext.Session.Remove("userLastName");
        HttpContext.Session.Remove("userEmail");
        return Redirect("/");
    }

    [HttpGet]
    [Route("/account/google-login")]
    public IActionResult GoogleLogin()
    {
        Console.WriteLine(Url.Action("GoogleResponse"));

        var authProperties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleResponse")
        };
        return Challenge(authProperties, GoogleDefaults.AuthenticationScheme);
    }


    [Route("/account/google-response")]
    public  async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded)
        {
            return RedirectToAction("Login");
        }

        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var userFound = _context.Users.FirstOrDefault(x => x.Email == email);

        if (userFound == null)
        {
            var user = new User(result.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "",
                result.Principal.FindFirstValue(ClaimTypes.Surname) ?? "", email ?? "",
                Hashing.HashPassword($"{result.Principal.FindFirstValue(ClaimTypes.NameIdentifier)}{DateTime.Now}"),
                Hashing.HashPassword($"{result.Principal.FindFirstValue(ClaimTypes.NameIdentifier)}{DateTime.Now}"),
                DateTime.Now.ToString(CultureInfo.CurrentCulture));
            await _context.AddAsync(user);
            await _context.SaveChangesAsync();
            userFound = user;
        }

        HttpContext.Session.SetString("userFirstName", userFound.FirstName ?? "");
        HttpContext.Session.SetString("userLastName", userFound.LastName ?? "");
        HttpContext.Session.SetString("userEmail", userFound.Email ?? "");
       

        return Redirect("/");
}

}