using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Transactions.Application;
using PersonalFinanceManager.Transactions.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=localhost,1433;Database=FinanceTransactionsDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"));

builder.Services.AddScoped<TransactionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapPost("/api/v1/transactions", async (CreateTransactionRequest request, TransactionService transactionService, CancellationToken cancellationToken) =>
{
    var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var created = await transactionService.CreateAsync(request, userId, cancellationToken);
    return Results.Created($"/api/v1/transactions/{created.Id}", created);
});

app.Run();
