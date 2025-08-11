using rinha_backend_2025.Api;

namespace rinha_backend_2025.Services;

public class PaymentProcessingService(ILogger<PaymentProcessingService> logger) {

    private readonly HttpClient _defaultClient = new() {
        BaseAddress = new Uri(Environment.GetEnvironmentVariable("PROCESSOR_DEFAULT_URL")!)
    };
    
    private readonly HttpClient _fallbackClient = new() {
        BaseAddress = new Uri(Environment.GetEnvironmentVariable("PROCESSOR_FALLBACK_URL")!)
    };
    
    public async Task ProcessAsync(PaymentRequest paymentRequest) {
        logger.LogInformation("Processing payment request: {CorrelationId}", paymentRequest.CorrelationId);

        if (await ProcessPaymentAsync(_defaultClient, paymentRequest)) {
            return;
        }
        
        logger.LogWarning("Default payment processor failed, trying fallback processor for request: {CorrelationId}", 
            paymentRequest.CorrelationId);

        if (await ProcessPaymentAsync(_fallbackClient, paymentRequest)) {
            return;
        }
    
        logger.LogError("Both payment processors failed for request: {CorrelationId}", paymentRequest.CorrelationId);

    }

    private static async Task<bool> ProcessPaymentAsync(HttpClient httpClient, PaymentRequest paymentRequest) {
        var req = await httpClient.PostAsJsonAsync("/payments", paymentRequest);
        return req.IsSuccessStatusCode;
    }

}