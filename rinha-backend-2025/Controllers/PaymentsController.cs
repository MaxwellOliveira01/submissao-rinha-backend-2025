using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using rinha_backend_2025.Api;
using rinha_backend_2025.Services;

namespace rinha_backend_2025.Controllers;

[ApiController]
public class PaymentsController(
    PaymentStatisticsService paymentStatisticsService,
    Channel<PaymentRequest> paymentChannel
) : ControllerBase {

    [HttpPost("payments")]
    public async Task<IActionResult> ProcessPayment(PaymentRequest request) {
        request.RequestedAt = DateTimeOffset.Now;
        await paymentChannel.Writer.WriteAsync(request);
        return Ok();
    }

    [HttpGet("payments-summary")]
    public async Task<PaymentSummary> GetSummary(DateTimeOffset from, DateTimeOffset to) {
        return await paymentStatisticsService.GetPaymentSummary(from, to);
    }
    
}