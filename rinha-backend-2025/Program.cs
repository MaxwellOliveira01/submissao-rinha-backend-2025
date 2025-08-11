using System.Threading.Channels;
using rinha_backend_2025.Api;
using rinha_backend_2025.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<PaymentProcessingService>();

builder.Services.AddSingleton<PaymentStatisticsService>();
builder.Services.AddSingleton<PaymentProcessingService>();

builder.Services.AddSingleton<Channel<PaymentRequest>>(
    _ => Channel.CreateUnbounded<PaymentRequest>(new UnboundedChannelOptions {
        SingleReader = false,
        SingleWriter = false,
        AllowSynchronousContinuations = false, 
    })
);

builder.Services.AddHostedService<PaymentQueueService>();

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
