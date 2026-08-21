using Microsoft.EntityFrameworkCore;
using Serilog;
using TutoringHub.API.Extensions;
using TutoringHub.API.Middleware;
using TutoringHub.Application.Services.Interfaces;
using TutoringHub.Infrastructure.Persistence;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Async(a => a.File("logs/tutoringhub-.log", rollingInterval: RollingInterval.Day))
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .WriteTo.Console()
            .WriteTo.Async(a => a.File("logs/tutoringhub-.log", rollingInterval: RollingInterval.Day)));

    builder.Services.AddControllers();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddRateLimitingPolicy();
    builder.Services.AddCorsPolicy(builder.Configuration);
    builder.Services.AddSwaggerWithJwt();
    builder.Services.AddTransient<GlobalExceptionHandler>();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();

        var seeder = scope.ServiceProvider.GetRequiredService<ISeedingService>();
        await seeder.SeedDefaultTeacherAsync(
            builder.Configuration["DefaultAdminPassword"] ?? "admin123");
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<GlobalExceptionHandler>();
    app.UseHttpsRedirection();
    app.UseCors("TutoringHubCorsPolicy");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}