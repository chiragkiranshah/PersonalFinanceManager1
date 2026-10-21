using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PersonalFinanceManager.Budgets.Application;
using PersonalFinanceManager.Budgets.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddDbContextCheck<BudgetDbContext>();

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

builder.Services.AddDbContext<BudgetDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=localhost,1433;Database=FinanceBudgetsDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"));

builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<BudgetService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapGet("/api/v1/budgets/{id:guid}", async (Guid id, BudgetService service, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    var userId = GetCurrentUserId(httpContext);
    var result = await service.GetAsync(id, userId, cancellationToken);
    return result is null ? Results.NotFound() : Results.Ok(result);
}).RequireAuthorization();

app.MapGet("/api/v1/budgets", async (BudgetService service, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    var userId = GetCurrentUserId(httpContext);
    return Results.Ok(await service.ListAsync(userId, cancellationToken));
}).RequireAuthorization();

app.MapPost("/api/v1/budgets", async (CreateBudgetRequest request, BudgetService service, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    var userId = GetCurrentUserId(httpContext);
    try
    {
        var budget = await service.CreateAsync(request, userId, cancellationToken);
        return Results.Created($"/api/v1/budgets/{budget.Id}", budget);
    }
    catch (ArgumentException ex)
    {
        return Results.ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            ["request"] = new[] { ex.Message }
        }));
    }
}).RequireAuthorization();

app.MapPut("/api/v1/budgets/{id:guid}", async (Guid id, UpdateBudgetRequest request, BudgetService service, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    var userId = GetCurrentUserId(httpContext);
    try
    {
        var result = await service.UpdateAsync(id, request, userId, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            ["request"] = new[] { ex.Message }
        }));
    }
}).RequireAuthorization();

app.MapDelete("/api/v1/budgets/{id:guid}", async (Guid id, BudgetService service, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    var userId = GetCurrentUserId(httpContext);
    var deleted = await service.DeleteAsync(id, userId, cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization();

app.Run();

static Guid GetCurrentUserId(HttpContext httpContext)
{
    var claim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? httpContext.User.FindFirst("sub")?.Value
        ?? httpContext.User.FindFirst("userId")?.Value;
    return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
}
