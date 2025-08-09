namespace rinha_backend_2025.Api;

public class PaymentSummary {

    public PaymentSummaryItem Default { get; set; } = new();

    public PaymentSummaryItem Fallback { get; set; } = new();
    
}

public class PaymentSummaryItem {
    public int TotalRequests { get; set; } = 0;
    public double TotalAmount { get; set; } = 0.0;
}

public class PaymentItem {
    public double TotalAmount { get; set; } = 0.0;
    public DateTimeOffset RequestedAt { get; set; }
}