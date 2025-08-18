using System.Text.Json;
using rinha_backend_2025.Api;
using StackExchange.Redis;

namespace rinha_backend_2025.Services;

public class PaymentStatisticsService(
    IConnectionMultiplexer redis,
    ILogger<PaymentStatisticsService> logger 
) {

    private const string DefaultKey = "default";
    private const string FallbackKey = "fallback";

    private readonly IDatabase _redisDatabase = redis.GetDatabase();

    public async Task AddPaymentToDefaultAsync(PaymentRequest paymentRequest)
        => await AddPaymentAsync(paymentRequest, DefaultKey);
    
    public async Task AddPaymentToFallbackAsync(PaymentRequest paymentRequest) 
        => await AddPaymentAsync(paymentRequest, FallbackKey);
    
    private async Task AddPaymentAsync(PaymentRequest paymentRequest, string processorKey) {
        var statistic = new PaymentStatistic {
            CorrelationId = paymentRequest.CorrelationId,
            Amount = paymentRequest.Amount,
        };
        
        var json = JsonSerializer.Serialize(statistic);

        await _redisDatabase.SortedSetAddAsync(processorKey, json, paymentRequest.RequestedAt.Ticks);
    }
    
    public async Task<PaymentSummary> GetPaymentSummary(DateTimeOffset from, DateTimeOffset to) {
        var fromTicks = from.Ticks;
        var toTicks = to.Ticks;

        var defaultPayments = FetchPaymentsAsync(DefaultKey, fromTicks, toTicks);
        var fallbackPayments = FetchPaymentsAsync(FallbackKey, fromTicks, toTicks);

        await Task.WhenAll(defaultPayments, fallbackPayments);

        return new PaymentSummary {
            Default = defaultPayments.Result,
            Fallback = fallbackPayments.Result,
        };
    }

    private async Task<PaymentSummaryItem> FetchPaymentsAsync(string queueName, long from, long to) {
        var payments = await _redisDatabase
            .SortedSetRangeByScoreAsync(queueName, from, to);

        return new PaymentSummaryItem {
            TotalRequests = payments.Length,
            TotalAmount = payments.Select(p => GetAmount(p!)).Sum()
        };
    }

    private static double GetAmount(string message) {
        var payment = JsonSerializer.Deserialize<PaymentStatistic>(message);
        return payment?.Amount ?? 0.0;
    }
    
    private class PaymentStatistic {    
        public Guid CorrelationId { get; set; }
        public double Amount { get; set; } = 0.0;
    }
    
}