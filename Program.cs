using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using practice.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using practice.Auth;
using practice.Repository;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var userSecretsConfig = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddUserSecrets<Program>()
            .Build();

builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
});

builder.Services.AddScoped<JWT>();
builder.Services.AddTransient<IItemRepository, ItemRepository>();
builder.Services.AddTransient<IOrderRepository, OrderRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
           .AddCookie(options =>
           {
               options.LoginPath = "/account/google-login";
               options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
               options.SlidingExpiration = true;
               options.AccessDeniedPath = "/Forbidden/";
           })
           .AddGoogle(options =>
           {
               options.ClientId = "320116085655-o5ddlr35dbjjlvsqaok83mm4tnb53ng1.apps.googleusercontent.com";
               options.ClientSecret = "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";
           });

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(
    options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "https://published.bsite.net/",
        ValidAudience = "https://published.bsite.net/",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("G2JCIW9PkUOiN47WjTRl"))
    };
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();
app.UseAuthentication();
app.UseCookiePolicy(new CookiePolicyOptions()
{
    MinimumSameSitePolicy = SameSiteMode.Lax
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");


app.Run();
