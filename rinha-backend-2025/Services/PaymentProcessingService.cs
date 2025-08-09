using System.Text;
using System.Text.Json;
using rinha_backend_2025.Api;

namespace rinha_backend_2025.Services;

public interface IPaymentProcessingService  {
    Task ProcessPaymentAsync(PaymentRequest request);
}

public class PaymentProcessingService : IPaymentProcessingService {

    private readonly IPaymentStatisticsService paymentStatisticsService;
    private readonly ILogger<PaymentProcessingService> logger;
    
    public PaymentProcessingService(IPaymentStatisticsService paymentStatisticsService, ILogger<PaymentProcessingService> logger) {
        this.paymentStatisticsService = paymentStatisticsService;
        this.logger = logger;
    }
    
    public async Task ProcessPaymentAsync(PaymentRequest request) {
        request.RequestedAt = DateTimeOffset.Now;

        var serverId = Environment.GetEnvironmentVariable("SERVER_ID");
        
        this.logger.LogInformation("Processing payment: {CorrelationId} with amount {Amount}", 
            request.CorrelationId, request.Amount);

        var tries = 0;

        var processorDefaultUrl = Environment.GetEnvironmentVariable("PROCESSOR_DEFAULT_URL");
        var processorFallbackUrl = Environment.GetEnvironmentVariable("PROCESSOR_FALLBACK_URL");
        
        while (!await tryProcessPaymentAsync(request, processorDefaultUrl)) {
            tries++;
            
            this.logger.LogWarning("Server [{ServerId}]: Payment processing failed: {CorrelationId} with amount {Amount}, attempt {Attempt}", 
                serverId, request.CorrelationId, request.Amount, tries);
            
            if (tries >= 5) {
                this.logger.LogError("Server [{ServerId}]: Payment processing failed after 3 attempts: {CorrelationId} with amount {Amount}", 
                    serverId, request.CorrelationId, request.Amount);
                return;
            }
            
            await Task.Delay(500);
        }
        
        this.logger.LogInformation("Server [{ServerId}]: Payment processed successfully: {CorrelationId} with amount {Amount}", 
            serverId, request.CorrelationId, request.Amount);

        paymentStatisticsService.AddToDefault(request);
        
    }

    private async Task<bool> tryProcessPaymentAsync(PaymentRequest request, string processorUrl) {
        using var httpClient = new HttpClient();
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(processorUrl + "/payments", content);
        return response.IsSuccessStatusCode;
    }
    
}