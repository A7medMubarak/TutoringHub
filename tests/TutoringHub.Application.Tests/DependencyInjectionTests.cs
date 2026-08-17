using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TutoringHub.API.Extensions;
using TutoringHub.API.Middleware;
using TutoringHub.Application.Services.Interfaces;

namespace TutoringHub.Application.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AllApplicationServices_ResolveWithoutError()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=NotReal;Trusted_Connection=True",
                ["Jwt:Key"] = "test-secret-key-with-at-least-32-characters!!",
                ["Jwt:Issuer"] = "TutoringHub.Tests",
                ["Jwt:Audience"] = "TutoringHub.Tests",
                ["Jwt:AccessTokenMinutes"] = "60",
                ["Jwt:RefreshTokenDays"] = "14"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddApplicationServices(configuration);
        services.AddJwtAuthentication(configuration);
        services.AddRateLimitingPolicy();
        services.AddTransient<GlobalExceptionHandler>();

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var resolved = new object?[]
        {
            scope.ServiceProvider.GetService<ICurrentUserService>(),
            scope.ServiceProvider.GetService<ITokenService>(),
            scope.ServiceProvider.GetService<IAuthService>(),
            scope.ServiceProvider.GetService<IStudentService>(),
            scope.ServiceProvider.GetService<ICenterService>(),
            scope.ServiceProvider.GetService<IClassGroupService>(),
            scope.ServiceProvider.GetService<ISeedingService>(),
            scope.ServiceProvider.GetService<GlobalExceptionHandler>()
        };

        resolved.Should().NotContainNulls("every registered service must be resolvable at startup");
    }
}