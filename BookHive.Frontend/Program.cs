using Microsoft.AspNetCore.Authentication.Cookies;
using BookHive.Frontend.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = false;
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<BackendAuthHandler>();

builder.Services.AddHttpClient("BackendApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5254/");
})
.AddHttpMessageHandler<BackendAuthHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program
{
}
