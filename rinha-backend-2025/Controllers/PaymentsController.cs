using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using rinha_backend_2025.Api;
using rinha_backend_2025.Services;
using StackExchange.Redis;

namespace rinha_backend_2025.Controllers;

[ApiController]
public class PaymentsController(
    PaymentStatisticsService paymentStatisticsService,
    IConnectionMultiplexer redis
    // Channel<PaymentRequest> paymentChannel
) : ControllerBase {

    private readonly IDatabase db = redis.GetDatabase();
    
    [HttpPost("payments")]
    public async Task<IActionResult> ProcessPayment(PaymentRequest request) {
        request.RequestedAt = DateTimeOffset.Now;
        var json = JsonSerializer.Serialize(request);
        await db.ListRightPushAsync("fila", json); // pegar do environment
        // await paymentChannel.Writer.WriteAsync(request);
        return Ok();
    }

    [HttpGet("payments-summary")]
    public async Task<PaymentSummary> GetSummary(DateTimeOffset from, DateTimeOffset to) {
        return await paymentStatisticsService.GetPaymentSummary(from, to);
    }
    
}