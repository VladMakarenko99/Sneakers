using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Sneakers.Data;
using Sneakers.Auth;
using Sneakers.Repository;
using Sneakers.Interfaces;
using Microsoft.AspNetCore.Authentication.Google;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
});

builder.Services.AddScoped<Auth>();
builder.Services.AddTransient<IItemRepository, ItemRepository>();
builder.Services.AddTransient<IOrderRepository, OrderRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();


builder.Services.AddAuthentication(options =>{
     options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
           .AddCookie(
            options =>
           {
               options.LoginPath = "/account/google-login";
               options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
               options.SlidingExpiration = true;
               options.AccessDeniedPath = "/Forbidden/";
           }
           )
           .AddGoogle(options =>
           {
               options.ClientId = builder.Configuration["Google:ClientId"]!;
               options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
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
