using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Hangfire;
using Hangfire.PostgreSql;
using StackExchange.Redis;
using System.Text;
using NexaGram.Infrastructure.Persistence;
using NexaGram.Infrastructure.Services.Ai;
using NexaGram.Infrastructure.Services.Auth;
using NexaGram.Infrastructure.Services.Engagement;
using NexaGram.Infrastructure.Services.Feed;
using NexaGram.Infrastructure.Services.Stories;
using NexaGram.Infrastructure.Services.Posts;
using NexaGram.Infrastructure.Services.Storage;
using NexaGram.Infrastructure.Services.Users;
using NexaGram.Application.Interfaces;

namespace NexaGram.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core + pgvector
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                o => o.UseVector()));

        // ASP.NET Core Identity
        services.AddIdentity<Domain.Entities.User, Domain.Entities.AppRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<DbSeeder>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IFeedService, FeedService>();
        services.AddScoped<IEngagementService, EngagementService>();
        services.AddScoped<IStoryService, StoryService>();
        services.AddSingleton<MinioStorageService>();
        services.AddSingleton<IStorageService>(sp => sp.GetRequiredService<MinioStorageService>());

        // JWT Authentication
        var jwtConfig = configuration.GetSection("Jwt");
        var secret = jwtConfig["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig["Issuer"],
                    ValidAudience = jwtConfig["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ClockSkew = TimeSpan.Zero
                };
                // SignalR support — read token from query string
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var accessToken = ctx.Request.Query["access_token"];
                        var path = ctx.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            ctx.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        // Redis
        var redisConn = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        var redisOptions = ConfigurationOptions.Parse(redisConn);
        redisOptions.AbortOnConnectFail = false;
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisOptions));

        // Hangfire
        services.AddHangfire(c => c
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(
                configuration.GetConnectionString("Hangfire"),
                new PostgreSqlStorageOptions { SchemaName = "hangfire" }));
        services.AddHangfireServer();

        // AI Service (provider pattern)
        var aiProvider = configuration["Ai:Provider"] ?? "ollama";
        if (aiProvider.Equals("claude", StringComparison.OrdinalIgnoreCase))
            services.AddHttpClient<IAiService, ClaudeAiService>();
        else
            services.AddHttpClient<IAiService, OllamaAiService>();

        // Health checks
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("DefaultConnection")!)
            .AddRedis(redisConn);

        return services;
    }
}
