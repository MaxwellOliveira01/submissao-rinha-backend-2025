using System.ComponentModel.DataAnnotations;

namespace rinha_backend_2025.Api;

public class PaymentRequest {
    
    [Required]
    public Guid CorrelationId { get; set; }
    
    [Required]
    public double Amount { get; set; }
    
    public DateTimeOffset RequestedAt { get; set; }
    
}