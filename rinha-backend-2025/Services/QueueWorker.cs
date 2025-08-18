using System.Threading.Channels;
using rinha_backend_2025.Api;
using StackExchange.Redis;

namespace rinha_backend_2025.Services;

public class QueueWorker(
    // Channel<PaymentRequest> paymentChannel,
    IConnectionMultiplexer redis,
    IServiceProvider serviceProvider,
    ILogger<QueueWorker> logger
) : BackgroundService {
    
    private readonly IDatabase db = redis.GetDatabase();
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        logger.LogInformation("PaymentQueueService started");

        var workersCount = int.Parse(Environment.GetEnvironmentVariable("WORKERS_COUNT") ?? "10");
        var workers = new Task[workersCount];
        
        for (var i = 0; i < workersCount; i++) {
            workers[i] = Task.Run(() => 
                WorkerLoopAsync(stoppingToken).ConfigureAwait(
                    continueOnCapturedContext: false
                ), stoppingToken
            );
        }
     
        await Task.WhenAll(workers);
    }

    private async Task WorkerLoopAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            var message = await db.ListLeftPopAsync("fila")
                .ConfigureAwait(continueOnCapturedContext: false);

            if (message.HasValue) {
                using var scope = serviceProvider.CreateScope();
                
                var paymentProcessingService = scope.ServiceProvider
                    .GetRequiredService<PaymentProcessingService>();
                
                await paymentProcessingService.ProcessAsync(message!)
                    .ConfigureAwait(continueOnCapturedContext: false);;
            }

            await Task.Delay(5, stoppingToken)
                .ConfigureAwait(continueOnCapturedContext: false);
        }
    }
    
}