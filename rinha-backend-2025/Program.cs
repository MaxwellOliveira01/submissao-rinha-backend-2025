using System.Threading.Channels;
using rinha_backend_2025.Api;
using rinha_backend_2025.Services;

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

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
