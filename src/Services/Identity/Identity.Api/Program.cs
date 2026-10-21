using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using PersonalFinanceManager.Identity.Api;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration).WriteTo.Console());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<IUserStore, InMemoryUserStore>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "PersonalFinanceManager.Identity",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "PersonalFinanceManager.Api",
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "development-secret-key-please-change"))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapPost("/api/v1/auth/register", async (RegisterRequest request, IUserStore userStore, IJwtTokenService tokenService) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.ValidationProblem(new ValidationProblemExceptionResult(new Dictionary<string, string[]>
        {
            ["request"] = new[] { "Email and password are required." }
        }));
    }

    var existingUser = await userStore.FindByEmailAsync(request.Email);
    if (existingUser is not null)
    {
        return Results.Conflict(new { message = "A user with that email already exists." });
    }

    var user = new AppUser(
        Guid.NewGuid(),
        request.Email.Trim(),
        string.IsNullOrWhiteSpace(request.UserName) ? request.Email.Trim() : request.UserName.Trim(),
        request.FirstName.Trim(),
        request.LastName.Trim(),
        PasswordHasher.Hash(request.Password));

    await userStore.AddAsync(user);
    var token = tokenService.CreateToken(user);
    var expiresAt = DateTime.UtcNow.AddHours(2);

    return Results.Ok(new AuthResponse(user.Id, user.Email, token, expiresAt));
});

app.MapPost("/api/v1/auth/login", async (LoginRequest request, IUserStore userStore, IJwtTokenService tokenService) =>
{
    var user = await userStore.FindByEmailAsync(request.Email);
    if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var token = tokenService.CreateToken(user);
    return Results.Ok(new AuthResponse(user.Id, user.Email, token, DateTime.UtcNow.AddHours(2)));
});

app.MapGet("/api/v1/users/me", async (HttpContext httpContext, IUserStore userStore) =>
{
    var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? httpContext.User.FindFirst("sub")?.Value;

    if (!Guid.TryParse(userIdClaim, out var userId))
    {
        return Results.Unauthorized();
    }

    var user = await userStore.FindByIdAsync(userId);
    return user is null
        ? Results.NotFound()
        : Results.Ok(new UserProfileResponse(user.Id, user.Email, user.UserName, user.FirstName, user.LastName));
}).RequireAuthorization();

app.Run();
