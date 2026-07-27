using System.Text;
using ChessPlatform.Features.Auth;
using ChessPlatform.Features.Users;
using ChessPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = Environment.GetEnvironmentVariable("JwtSettings__Issuer"),
        ValidAudience = Environment.GetEnvironmentVariable("JwtSettings__Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JwtSettings__Key")!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
    };
});

builder.Services.AddAuthorization();

builder.Services.AddDbContext<ChessDbContext>(options =>
{
    options.UseNpgsql(Environment.GetEnvironmentVariable("DefaultConnection"));
});

builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();