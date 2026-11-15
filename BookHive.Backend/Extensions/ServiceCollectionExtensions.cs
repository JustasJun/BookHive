using BookHive.Backend.Repositories;
using BookHive.Services;
using Microsoft.AspNetCore.Identity;
using BookHive.Models;

namespace BookHive.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBookHiveRepositories(this IServiceCollection services)
    {
        services.AddScoped<IGoalRepository, GoalRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILibraryRepository, LibraryRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IFollowRepository, FollowRepository>();

        return services;
    }

    public static IServiceCollection AddBookHiveServices(this IServiceCollection services)
    {
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<ILibraryService, LibraryService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IGoalService, GoalService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IFollowService, FollowService>();
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddSingleton<IOnlineUsersTracker, OnlineUsersTracker>();

        return services;
    }

    public static IServiceCollection AddOpenLibraryClient(this IServiceCollection services)
    {
        services.AddHttpClient<IOpenLibraryClient, OpenLibraryClient>(client =>
        {
            client.BaseAddress = new Uri("https://openlibrary.org/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("BookHive/1.0 (studentu projektas)");
        });

        return services;
    }
}
