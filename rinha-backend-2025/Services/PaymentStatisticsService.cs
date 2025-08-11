using rinha_backend_2025.Api;

namespace rinha_backend_2025.Services;

public class PaymentStatisticsService {

    // private PaymentSummaryItem defaultSummary = new();

    private List<PaymentItem> defaultPayments = new();
    
    private PaymentSummaryItem fallbackSummary = new();
    
    
    public void AddToDefault(PaymentRequest paymentRequest) {
        defaultPayments.Add(new() {
            TotalAmount = paymentRequest.Amount,
            RequestedAt = paymentRequest.RequestedAt
        });
    }
    
    // public void AddToFallback(double amount) {
    //     fallbackSummary.TotalAmount += amount;
    //     fallbackSummary.TotalRequests += 1;
    // }

    public PaymentSummary GetPaymentSummary(DateTimeOffset from, DateTimeOffset to) {
        
        var paymentsInRange = defaultPayments
            .Where(p => p.RequestedAt >= from && p.RequestedAt < to)
            .ToList();

        var summary = new PaymentSummaryItem() {
            TotalRequests = paymentsInRange.Count(),
            TotalAmount = paymentsInRange.Sum(p => p.TotalAmount)
        };
        
        return new PaymentSummary() {
            Default = summary,
            Fallback = new(),
        };
    }

}