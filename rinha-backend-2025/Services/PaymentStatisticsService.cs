using System.Text.Json;
using rinha_backend_2025.Api;
using StackExchange.Redis;

namespace rinha_backend_2025.Services;

public class PaymentStatisticsService(
    IConnectionMultiplexer redis,
    ILogger<PaymentStatisticsService> logger 
) {

    private readonly IDatabase _redisDatabase = redis.GetDatabase();

    public async Task AddPaymentToDefaultAsync(PaymentRequest paymentRequest)
        => await AddPayment(paymentRequest, "default"); // TODO: criar uma enum
    
    public async Task AddPaymentToFallbackAsync(PaymentRequest paymentRequest) 
        => await AddPayment(paymentRequest, "fallback");
    
    private async Task AddPayment(PaymentRequest paymentRequest, string processorKey) {
        var statistic = new PaymentStatistic {
            CorrelationId = paymentRequest.CorrelationId,
            Amount = paymentRequest.Amount,
            Processor = processorKey
        };
        
        var json = JsonSerializer.Serialize(statistic);
        
        await _redisDatabase.SortedSetAddAsync("payments", json, paymentRequest.RequestedAt.Ticks);
    }
    
    public async Task<PaymentSummary> GetPaymentSummary(DateTimeOffset from, DateTimeOffset to) {
        var fromTicks = from.Ticks;
        var toTicks = to.Ticks;

        var payments = await _redisDatabase
            .SortedSetRangeByScoreAsync("payments", fromTicks, toTicks);

        PaymentSummaryItem defaultProcessor = new();
        PaymentSummaryItem fallbackProcessor = new();
        
        foreach (var paymentRaw in payments) {

            if (string.IsNullOrEmpty(paymentRaw)) {
                continue; // should not happen
            }
            
            var payment = JsonSerializer.Deserialize<PaymentStatistic>(paymentRaw!);
            
            switch (payment!.Processor) {
                case "default":
                    defaultProcessor.TotalAmount += payment.Amount;
                    defaultProcessor.TotalRequests++;
                    break;
                case "fallback":
                    fallbackProcessor.TotalAmount += payment.Amount;
                    fallbackProcessor.TotalRequests++;
                    break;
                default:
                    logger.LogWarning("Unknown processor type: {Processor}", payment.Processor);
                    break;
            }
            
        }

        return new PaymentSummary {
            Default = defaultProcessor,
            Fallback = fallbackProcessor
        };

    }

    private class PaymentStatistic {
        public Guid CorrelationId { get; set; }
        public double Amount { get; set; } = 0.0;
        public string Processor { get; set; } = string.Empty;
    }
    
}