using LabWebApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
ConfigurationManager configuration = builder.Configuration;
// Step 1. Add Authentication Mechanisms
services
    .AddAuthentication(o =>
    {
        o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(o =>
    {
        // set the path for the authentication challenge
        o.LoginPath = "/signin";
        // set the path for the sign out
        o.LogoutPath = "/signout";
    })
    .AddGitHub(o =>
    {
        o.ClientId = configuration["Authentication:GitHub:clientId"];
        o.ClientSecret = configuration["Authentication:GitHub:clientSecret"];
        o.CallbackPath = "/signin-github";

        // Grants access to read a user's profile data.
        // https://docs.github.com/en/developers/apps/building-oauth-apps/scopes-for-oauth-apps
        o.Scope.Add("read:user");

        // Optional
        // if you need an access token to call GitHub Apis
        o.Events.OnCreatingTicket += context =>
        {
            if (context.AccessToken is { })
            {
                context.Identity?.AddClaim(new Claim("access_token", context.AccessToken));
            }

            return Task.CompletedTask;
        };
    });

services.AddRazorPages();
//end addition

// Add services to the container.
builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("ProductDBConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
//Configure Identity
Identity: builder.Services.AddDefaultIdentity<IdentityUser>().AddEntityFrameworkStores<ApplicationDbContext>();


//Connections Stories
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
// Add authentication middleware
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
