using BookHive.Backend.Data;
using BookHive.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BookHiveDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("BookHiveDatabase")));

builder.Services.AddControllers();
builder.Services
    .AddBookHiveRepositories()
    .AddBookHiveServices()
    .AddOpenLibraryClient();    

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5176")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();