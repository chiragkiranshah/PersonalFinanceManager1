using System.Security.Claims;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PersonalFinanceManager.Notifications.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
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

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BudgetThresholdExceededConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:User"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapPost("/api/v1/notifications/preferences", (NotificationPreference preference, HttpContext httpContext) =>
{
    var userId = GetCurrentUserId(httpContext);
    var item = NotificationPreference.Create(userId, preference.Email, preference.EmailEnabled, preference.PushEnabled);
    return Results.Ok(new { item.Id, item.UserId, item.Email, item.EmailEnabled, item.PushEnabled });
}).RequireAuthorization();

app.Run();

static Guid GetCurrentUserId(HttpContext httpContext)
{
    var claim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? httpContext.User.FindFirst("sub")?.Value
        ?? httpContext.User.FindFirst("userId")?.Value;
    return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
}

public class BudgetThresholdExceededConsumer : IConsumer<BudgetThresholdExceededEvent>
{
    public Task Consume(ConsumeContext<BudgetThresholdExceededEvent> context)
    {
        var evt = context.Message;
        Console.WriteLine($"Budget threshold exceeded for user {evt.UserId}: {evt.UtilizationPercent}% utilized");
        return Task.CompletedTask;
    }
}
