using BookHive.Backend.Repositories;
using BookHive.Services;

namespace BookHive.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBookHiveRepositories(this IServiceCollection services)
    {
        services.AddScoped<IGoalRepository, GoalRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILibraryRepository, LibraryRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();

        return services;
    }

    public static IServiceCollection AddBookHiveServices(this IServiceCollection services)
    {
        services.AddScoped<BookService>();
        services.AddScoped<LibraryService>();
        services.AddScoped<UserService>();
        services.AddScoped<GoalService>();
        services.AddScoped<ReviewService>();

        return services;
    }

    public static IServiceCollection AddOpenLibraryClient(this IServiceCollection services)
    {
        services.AddHttpClient<OpenLibraryClient>(client =>
        {
            client.BaseAddress = new Uri("https://openlibrary.org/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("BookHive/1.0 (studentu projektas)");
        });

        return services;
    }
}