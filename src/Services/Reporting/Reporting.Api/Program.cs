using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PersonalFinanceManager.Reporting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

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

builder.Services.AddScoped<IReportingRepository, InMemoryReportingRepository>();
builder.Services.AddScoped<ReportingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapGet("/api/v1/reports/summary", async (ReportingService service, HttpContext httpContext, [FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc, CancellationToken cancellationToken) =>
{
    var userId = GetCurrentUserId(httpContext);
    var summary = await service.GetFinancialSummaryAsync(userId, fromUtc, toUtc, cancellationToken);
    return Results.Ok(summary);
}).RequireAuthorization();

app.Run();

static Guid GetCurrentUserId(HttpContext httpContext)
{
    var claim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? httpContext.User.FindFirst("sub")?.Value
        ?? httpContext.User.FindFirst("userId")?.Value;
    return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
}

public sealed class InMemoryReportingRepository : IReportingRepository
{
    public Task<decimal> GetTotalIncomeAsync(Guid userId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) => Task.FromResult(12500m);
    public Task<decimal> GetTotalExpensesAsync(Guid userId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) => Task.FromResult(8600m);
    public Task<decimal> GetBudgetAmountAsync(Guid userId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) => Task.FromResult(11000m);
}
