using ComplainatorAPI.Domain.Settings;
using ComplainatorAPI.Services;

namespace ComplainatorAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure JwtSettings
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            // Register services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRetrospectiveService, RetrospectiveService>();
            services.AddScoped<IAISuggestionService, MockAISuggestionService>();

            return services;
        }
    }
} 