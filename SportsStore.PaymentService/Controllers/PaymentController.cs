using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStore.PaymentService.Data;

namespace SportsStore.PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        PaymentDbContext context,
        ILogger<PaymentController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("transactions")]
    public async Task<ActionResult> GetTransactions(
        [FromQuery] int? orderId = null,
        [FromQuery] string? status = null)
    {
        _logger.LogInformation("GetTransactions endpoint called - OrderId: {OrderId}, Status: {Status}", orderId, status);

        var query = _context.PaymentTransactions.AsQueryable();

        if (orderId.HasValue)
        {
            query = query.Where(t => t.OrderId == orderId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status == status);
        }

        var transactions = await query
            .OrderByDescending(t => t.CreatedAt)
            .Take(100)
            .ToListAsync();

        return Ok(transactions);
    }

    [HttpGet("transactions/{transactionId}")]
    public async Task<ActionResult> GetTransaction(int transactionId)
    {
        _logger.LogInformation("GetTransaction endpoint called - TransactionId: {TransactionId}", transactionId);

        var transaction = await _context.PaymentTransactions
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

        if (transaction == null)
            return NotFound();

        return Ok(transaction);
    }

    [HttpGet("test-cards")]
    public async Task<ActionResult> GetTestCards()
    {
        _logger.LogInformation("GetTestCards endpoint called");

        var cards = await _context.TestCards.ToListAsync();
        return Ok(cards);
    }

    [HttpGet("health")]
    public ActionResult Health()
    {
        return Ok(new
        {
            Service = "PaymentService",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }
}
