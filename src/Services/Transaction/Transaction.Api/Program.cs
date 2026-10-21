using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PersonalFinanceManager.Transactions.Application;
using PersonalFinanceManager.Transactions.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TransactionDbContext>();

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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost,1433;Database=FinanceTransactionsDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";

builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<TransactionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapGet("/api/v1/transactions/{id:guid}", async (Guid id, TransactionService transactionService, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    var userId = GetCurrentUserId(httpContext);
    var result = await transactionService.GetAsync(id, userId, cancellationToken);
    return result is null ? Results.NotFound() : Results.Ok(result);
}).RequireAuthorization();

app.MapGet("/api/v1/transactions", async (TransactionService transactionService, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    var userId = GetCurrentUserId(httpContext);
    var result = await transactionService.ListAsync(userId, cancellationToken);
    return Results.Ok(result);
}).RequireAuthorization();

app.MapPost("/api/v1/transactions", async (CreateTransactionRequest request, TransactionService transactionService, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    var userId = GetCurrentUserId(httpContext);
    try
    {
        var created = await transactionService.CreateAsync(request, userId, cancellationToken);
        return Results.Created($"/api/v1/transactions/{created.Id}", created);
    }
    catch (ArgumentException ex)
    {
        return Results.ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            ["request"] = new[] { ex.Message }
        }));
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest, title: "Transaction rejected");
    }
}).RequireAuthorization();

app.MapPost("/api/v1/transactions/transfers", async (CreateTransferRequest request, TransactionService transactionService, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    var userId = GetCurrentUserId(httpContext);
    try
    {
        var created = await transactionService.CreateTransferAsync(request, userId, cancellationToken);
        return Results.Created($"/api/v1/transactions/{created.Id}", created);
    }
    catch (ArgumentException ex)
    {
        return Results.ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            ["request"] = new[] { ex.Message }
        }));
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest, title: "Transfer rejected");
    }
}).RequireAuthorization();

app.MapPut("/api/v1/transactions/{id:guid}", async (Guid id, UpdateTransactionRequest request, TransactionService transactionService, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    var userId = GetCurrentUserId(httpContext);
    try
    {
        var updated = await transactionService.UpdateAsync(id, request, userId, cancellationToken);
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

app.MapDelete("/api/v1/transactions/{id:guid}", async (Guid id, TransactionService transactionService, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    var userId = GetCurrentUserId(httpContext);
    var deleted = await transactionService.DeleteAsync(id, userId, cancellationToken);
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
