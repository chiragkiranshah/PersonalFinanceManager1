using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PersonalFinanceManager.Accounts.Application;
using PersonalFinanceManager.Accounts.Infrastructure;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AccountDbContext>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "development-secret-key-please-change"))
        };
    });

builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost,1433;Database=FinanceAccountsDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";

builder.Services.AddDbContext<AccountDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<AccountService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapGet("/api/v1/accounts/{id:guid}", async (Guid id, AccountService accountService, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    var userId = GetCurrentUserId(httpContext);
    var result = await accountService.GetAsync(id, userId, cancellationToken);
    return result is null ? Results.NotFound() : Results.Ok(result);
}).RequireAuthorization();

app.MapGet("/api/v1/accounts", async (AccountService accountService, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    var userId = GetCurrentUserId(httpContext);
    var result = await accountService.ListAsync(userId, cancellationToken);
    return Results.Ok(result);
}).RequireAuthorization();

app.MapPost("/api/v1/accounts", async (CreateAccountRequest request, AccountService accountService, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    var userId = GetCurrentUserId(httpContext);
    try
    {
        var account = await accountService.CreateAsync(request, userId, cancellationToken);
        return Results.Created($"/api/v1/accounts/{account.Id}", account);
    }
    catch (ArgumentException ex)
    {
        return Results.ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            ["request"] = new[] { ex.Message }
        }));
    }
}).RequireAuthorization();

app.MapPut("/api/v1/accounts/{id:guid}", async (Guid id, UpdateAccountRequest request, AccountService accountService, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    var userId = GetCurrentUserId(httpContext);
    try
    {
        var updated = await accountService.UpdateAsync(id, request, userId, cancellationToken);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    }
    catch (ArgumentException ex)
    {
        return Results.ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            ["request"] = new[] { ex.Message }
        }));
    }
}).RequireAuthorization();

app.MapDelete("/api/v1/accounts/{id:guid}", async (Guid id, AccountService accountService, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    var userId = GetCurrentUserId(httpContext);
    try
    {
        var deleted = await accountService.DeleteAsync(id, userId, cancellationToken);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict, title: "Account deletion rejected");
    }
}).RequireAuthorization();

app.Run();

static Guid GetCurrentUserId(HttpContext httpContext)
{
    var claim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? httpContext.User.FindFirst("sub")?.Value
        ?? httpContext.User.FindFirst("userId")?.Value;

    return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
}
