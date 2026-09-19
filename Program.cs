using BookHive.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<OpenLibraryClient>(client =>
{
    client.BaseAddress = new Uri("https://openlibrary.org/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("BookHive/1.0 (studentu projektas)");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
    
// LAIKINAI: patikrinimui, prieš commit'ą ištrinti
app.MapGet("/test-books", async (OpenLibraryClient client) =>
    await client.SearchAsync("tolkien", limit: 5));


app.Run();