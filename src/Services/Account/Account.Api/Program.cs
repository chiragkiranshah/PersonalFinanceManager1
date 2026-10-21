using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Accounts.Application;
using PersonalFinanceManager.Accounts.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AccountDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=localhost,1433;Database=FinanceAccountsDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"));

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<AccountService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapGet("/api/v1/accounts/{id:guid}", async (Guid id, AccountService accountService, CancellationToken cancellationToken) =>
{
    var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var result = await accountService.GetAsync(id, userId, cancellationToken);
    return result is null ? Results.NotFound() : Results.Ok(result);
});

app.MapGet("/api/v1/accounts", async (AccountService accountService, CancellationToken cancellationToken) =>
{
    var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var result = await accountService.ListAsync(userId, cancellationToken);
    return Results.Ok(result);
});

app.MapPost("/api/v1/accounts", async (CreateAccountRequest request, AccountService accountService, CancellationToken cancellationToken) =>
{
    var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var account = await accountService.CreateAsync(request, userId, cancellationToken);
    return Results.Created($"/api/v1/accounts/{account.Id}", account);
});

app.Run();
