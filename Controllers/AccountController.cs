using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;
using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using practice.Data;
using practice.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json;
using System.Text;
using practice.Auth;
using practice.Repository;

namespace practice.Controllers;

public class AccountController : Controller
{
    private readonly UserRepository _repository;

    private readonly JWT _jwt;


    public AccountController(UserRepository _repository, JWT jwt)
    {
        this._jwt = jwt;
        this._repository = _repository;
    }


    [Route("/account/register")]
    [HttpGet]
    public IActionResult Register()
    {
        if (HttpContext.Session.GetString("JwtToken") != null)
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

        if (!string.IsNullOrEmpty(user.Email) && await _repository.GetByEmailAsync(user.Email) != null)
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
        user = new User(user.FirstName!, user.LastName!, user.Email!, Hashing.HashPassword($"{user.Password!}{salt}"), Hashing.HashPassword($"{user.ConfirmedPass!}{salt}"), salt);
        await _repository.AddAsync(user);

        HttpContext.Session.SetString("JwtToken", _jwt.GenerateJwtToken(user));

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
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (string.IsNullOrEmpty(model.Email))
            ModelState.AddModelError(nameof(model.Email), "This field is required!");

        if (string.IsNullOrEmpty(model.Password))
            ModelState.AddModelError(nameof(model.Password), "This field is required!");

        if (!ModelState.IsValid) return View("Login", model);


        var userFound = await _repository.GetByEmailAsync(model.Email!);

        if (Hashing.HashPassword($"{model.Password}{userFound?.Salt}") == userFound?.Password)
        {

            HttpContext.Session.SetString("JwtToken", _jwt.GenerateJwtToken(userFound));


            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("81efc4e3f15367", "57ba73d15a23d3"),
                EnableSsl = true
            };

            await ResetCartData(userFound);

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
        HttpContext.Session.Remove("JwtToken");


        return Redirect("/");
    }

    [Route("/profile")]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            return NotFound();

        var currentUser = _jwt.GetCurrentUser(HttpContext.Session.GetString("JwtToken")!);
        currentUser = await _repository.GetByEmailAsync(currentUser!.Email!);
        if (currentUser == null)
            return NotFound();

        return View(currentUser);
    }

    [Route("/account/delete")]
    public async Task<IActionResult> DeleteAccount()
    {
        string? token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
            return NotFound();
        var currentUser = _jwt.GetCurrentUser(token);
        await _repository.Remove(await _repository.GetByEmailAsync(currentUser!.Email!) ?? new User());
        HttpContext.Session.Remove("JwtToken");
        return Redirect("/");
    }

    [HttpGet]
    [Route("/account/google-login")]
    public IActionResult GoogleLogin()
    {
        var authProperties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleResponse")
        };
        return Challenge(authProperties, GoogleDefaults.AuthenticationScheme);
    }


    [Route("/account/google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded)
        {
            return RedirectToAction("Login");
        }

        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        System.Console.WriteLine(email);
        var userFound = await _repository.GetByEmailAsync(email!);

        if (userFound == null)
        {
            var user = new User(
                result.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "",
                result.Principal.FindFirstValue(ClaimTypes.Surname) ?? "",
                email ?? "",
                Hashing.HashPassword($"{result.Principal.FindFirstValue(ClaimTypes.NameIdentifier)}{DateTime.Now}"),
                Hashing.HashPassword($"{result.Principal.FindFirstValue(ClaimTypes.NameIdentifier)}{DateTime.Now}"),
                DateTime.Now.ToString(CultureInfo.CurrentCulture)
                );
            await _repository.AddAsync(user);
            userFound = user;
        }
        HttpContext.Session.SetString("JwtToken", _jwt.GenerateJwtToken(userFound));
        System.Console.WriteLine(_jwt.GenerateJwtToken(userFound));

        await ResetCartData(userFound);

        return Redirect("/");
    }

    private async Task ResetCartData(User user)
    {
        if (user.CartItemsJson == null)
        {
            var bytes = HttpContext.Session.Get("cartItems") ?? null;
            if (bytes != null)
            {
                string json = Encoding.UTF8.GetString(bytes);
                user.CartItemsJson = json;
                await _repository.Update(user);
                HttpContext.Session.SetString("JwtToken", _jwt.GenerateJwtToken(user));
            }
        }
        else
        {
            var list1 = JsonSerializer.Deserialize<List<Item>>(user.CartItemsJson)!;
            var bytes = HttpContext.Session.Get("cartItems") ?? Array.Empty<byte>();
            string json = Encoding.UTF8.GetString(bytes);
            var list2 = new List<Item>();

            if (!string.IsNullOrEmpty(json))
            {
                list2 = JsonSerializer.Deserialize<List<Item>>(json) ?? new List<Item>();
            }

            list1.AddRange(list2);
            int totalPrice = 0;
            foreach (var item in list1)
                totalPrice += item.Price;
            HttpContext.Session.SetString("cartItems", JsonSerializer.Serialize(list1));
            HttpContext.Session.SetInt32("LoginedUserProductCount", list1!.Count);
            HttpContext.Session.SetInt32("totalPrice", totalPrice);

            user.CartItemsJson = JsonSerializer.Serialize(list1);
            await _repository.Update(user);
            HttpContext.Session.SetString("JwtToken", _jwt.GenerateJwtToken(user));
        }
    }

    public async Task<IActionResult> UploadPhoto(User user, IFormFile file)
    {
        await _repository.UploadPhotoAsync(user, file);
        return Redirect("/profile");
    }

    [Route("/account/manage")]
    [HttpGet]
    public async Task<IActionResult> Manage()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            return NotFound();

        var currentUser = _jwt.GetCurrentUser(HttpContext.Session.GetString("JwtToken")!);
        currentUser = await _repository.GetByEmailAsync(currentUser!.Email!);
        if (currentUser == null)
            return NotFound();

        return View(currentUser);
    }

    [HttpPost]
    public async Task<IActionResult> Manage(User user)
    {
        if (string.IsNullOrEmpty(user.FirstName))
            ModelState.AddModelError(nameof(user.FirstName), "This field is required!");
            
        if (string.IsNullOrEmpty(user.LastName))
            ModelState.AddModelError(nameof(user.LastName), "This field is required!");

        if (!ModelState.IsValid) return View("Manage", user);

        var userToUpdate = await _repository.GetByEmailAsync(user.Email!) ?? new User();

        userToUpdate.FirstName = user.FirstName;
        userToUpdate.LastName = user.LastName;
        userToUpdate.Email = user.Email;

        await _repository.Update(userToUpdate);
        HttpContext.Session.SetString("JwtToken", _jwt.GenerateJwtToken(userToUpdate));

        return Redirect("/profile");
    }
}