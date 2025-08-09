using rinha_backend_2025.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();
builder.Services.AddSingleton<IPaymentStatisticsService, PaymentStatisticsService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
