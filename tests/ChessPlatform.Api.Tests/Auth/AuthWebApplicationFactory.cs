using ChessPlatform.Api.Domain.Entities;
using ChessPlatform.Api.Features.Auth;
using ChessPlatform.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChessPlatform.Api.Tests.Auth;

public class AuthWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "JwtSettings:Key", "this-is-a-long-test-key-with-more-than-32-chars" },
                { "JwtSettings:Issuer", "Test" },
                { "JwtSettings:Audience", "Test" }
            });
        });
        
        builder.ConfigureServices(services =>
        {
            services.Remove(
                services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<ChessDbContext>))!
            );
            
            services.AddDbContext<ChessDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });
            
            var serviceProvider = services.BuildServiceProvider();
            var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ChessDbContext>();
            
            var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();
            
            dbContext.Users.Add(new User
            {
                Username = "john",
                Email = "john@test.com",
                PasswordHash = passwordHasher.HashPassword("password123"),
            });

            dbContext.SaveChanges();
        });
    }
}