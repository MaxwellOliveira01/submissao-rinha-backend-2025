using Microsoft.AspNetCore.Mvc;
using rinha_backend_2025.Api;
using rinha_backend_2025.Services;

namespace rinha_backend_2025.Controllers;

[ApiController]
public class PaymentsController : ControllerBase {

    private readonly IPaymentProcessingService paymentProcessingService;
    private readonly IPaymentStatisticsService paymentStatisticsService;
    
    public PaymentsController(IPaymentProcessingService paymentProcessingService, IPaymentStatisticsService paymentStatisticsService) {
        this.paymentProcessingService = paymentProcessingService;
        this.paymentStatisticsService = paymentStatisticsService;
    }

    [HttpPost("payments")]
    public async Task<IActionResult> ProcessPayment(PaymentRequest request) {
        await paymentProcessingService.ProcessPaymentAsync(request);
        return Ok();
    }

    [HttpGet("payments-summary")]
    public async Task<PaymentSummary> GetSummary(DateTimeOffset from, DateTimeOffset to) {
        return paymentStatisticsService.GetPaymentSummary(from, to);
    }
    
}