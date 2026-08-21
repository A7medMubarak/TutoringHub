using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Text;
using TutoringHub.Application.Common.Options;
using TutoringHub.Application.Services;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.API.Services;
using TutoringHub.Domain.Interfaces;
using TutoringHub.Infrastructure.External;
using TutoringHub.Infrastructure.Persistence;
using TutoringHub.Infrastructure.Security;

namespace TutoringHub.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ICenterService, CenterService>();
        services.AddScoped<IClassGroupService, ClassGroupService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IQuotaService, QuotaService>();
        services.AddScoped<IAiService, AiService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IStudentQuizService, StudentQuizService>();
        services.AddScoped<IMeService, MeService>();
        services.AddScoped<ISeedingService, SeedingService>();

        services.AddHttpClient<IAiClient, GeminiAiClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(90);
        });

        services.AddValidatorsFromAssemblyContaining<IAuthService>();
        services.AddFluentValidationAutoValidation();
    }

    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var key = jwtSection["Key"];

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(key ?? throw new InvalidOperationException("Jwt:Key is not configured.")))
                };
            });
    }

    public static void AddRateLimitingPolicy(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("auth", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy("ai", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });
    }

    public static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("TutoringHubCorsPolicy", policy =>
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });
    }

    public static void AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }
}