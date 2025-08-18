using System.Threading.Channels;
using rinha_backend_2025.Api;
using rinha_backend_2025.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddTransient<PaymentProcessingService>();

builder.Services.AddSingleton<PaymentStatisticsService>();
builder.Services.AddSingleton<PaymentProcessingService>();

builder.Services.AddHostedService<QueueWorker>();

var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL");
if (string.IsNullOrEmpty(redisUrl)) {
    throw new Exception("REDIS_URL environment variable not set");
}

builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisUrl)
);

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
