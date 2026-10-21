using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Accounts.Application;
using PersonalFinanceManager.Accounts.Domain;
using PersonalFinanceManager.Accounts.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AccountDbContext>();

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

app.MapHealthChecks("/health");

app.MapGet("/api/v1/accounts/{id:guid}", async (Guid id, AccountService accountService, CancellationToken cancellationToken) =>
{
    var userId = ResolveCurrentUserId();
    var result = await accountService.GetAsync(id, userId, cancellationToken);
    return result is null ? Results.NotFound() : Results.Ok(result);
});

app.MapGet("/api/v1/accounts", async (AccountService accountService, CancellationToken cancellationToken) =>
{
    var userId = ResolveCurrentUserId();
    var result = await accountService.ListAsync(userId, cancellationToken);
    return Results.Ok(result);
});

app.MapPost("/api/v1/accounts", async (CreateAccountRequest request, AccountService accountService, CancellationToken cancellationToken) =>
{
    var userId = ResolveCurrentUserId();
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
});

app.MapPut("/api/v1/accounts/{id:guid}", async (Guid id, UpdateAccountRequest request, AccountService accountService, CancellationToken cancellationToken) =>
{
    var userId = ResolveCurrentUserId();
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
});

app.MapDelete("/api/v1/accounts/{id:guid}", async (Guid id, AccountService accountService, CancellationToken cancellationToken) =>
{
    var userId = ResolveCurrentUserId();
    var deleted = await accountService.DeleteAsync(id, userId, cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.Run();

static Guid ResolveCurrentUserId()
{
    return Guid.Parse("11111111-1111-1111-1111-111111111111");
}
