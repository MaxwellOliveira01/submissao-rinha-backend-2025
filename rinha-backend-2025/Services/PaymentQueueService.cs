using System.Threading.Channels;
using rinha_backend_2025.Api;

namespace rinha_backend_2025.Services;

public class PaymentQueueService(
    Channel<PaymentRequest> paymentChannel,
    PaymentProcessingService paymentProcessingService,
    ILogger<PaymentQueueService> logger
) : BackgroundService {
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        logger.LogInformation("PaymentQueueService started");

        var workersCount = 5; // todo: get from environment
        var tasks = new List<Task>();

        for (var i = 0; i < workersCount; i++) {
            var workerId = i;
            tasks.Add(Task.Run(async () => {
                while (await paymentChannel.Reader.WaitToReadAsync(stoppingToken)) {
                    var request = await paymentChannel.Reader.ReadAsync(stoppingToken);
                    logger.LogInformation("Worker [{WorkerId}]: Read request: {Request}", workerId, request.CorrelationId);
                    await paymentProcessingService.ProcessAsync(request);
                }    
            }, stoppingToken));
        }
     
        await Task.WhenAll(tasks);
    }
    
}