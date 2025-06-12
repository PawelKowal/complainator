using ComplainatorAPI.Domain.Settings;
using ComplainatorAPI.Services;
using ComplainatorAPI.Services.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ComplainatorAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure JwtSettings
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            // Configure OpenRouter
            services.Configure<OpenRouterSettings>(configuration.GetSection("OpenRouter"));

            // Register services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRetrospectiveService, RetrospectiveService>();
            services.AddScoped<IAISuggestionService, AISuggestionService>();
            services.AddScoped<ISuggestionService, SuggestionService>();

            // Add OpenRouter service with retry and circuit breaker policies
            services.AddOpenRouterService(options =>
            {
                options.ApiKey = configuration["OpenRouter:ApiKey"] ?? throw new InvalidOperationException("OpenRouter:ApiKey is not configured");
                options.EndpointUrl = configuration["OpenRouter:EndpointUrl"] ?? "https://openrouter.ai/api/v1/chat/completions";
                options.DefaultModel = configuration["OpenRouter:DefaultModel"] ?? "gpt-3.5-turbo";
            });

            return services;
        }
    }
} 