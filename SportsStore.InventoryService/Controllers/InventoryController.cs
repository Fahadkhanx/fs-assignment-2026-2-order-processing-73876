using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStore.InventoryService.Data;

namespace SportsStore.InventoryService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly InventoryDbContext _context;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(
        InventoryDbContext context,
        ILogger<InventoryController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        _logger.LogInformation("GetAll inventory items endpoint called");

        var items = await _context.InventoryItems
            .OrderBy(i => i.ProductName)
            .ToListAsync();

        return Ok(items.Select(i => new
        {
            i.ProductId,
            i.ProductName,
            i.Category,
            i.Price,
            i.StockQuantity,
            i.ReservedQuantity,
            AvailableQuantity = i.AvailableQuantity,
            i.LastUpdated
        }));
    }

    [HttpGet("{productId}")]
    public async Task<ActionResult> GetById(long productId)
    {
        _logger.LogInformation("GetById inventory endpoint called - ProductId: {ProductId}", productId);

        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (item == null)
            return NotFound();

        return Ok(new
        {
            item.ProductId,
            item.ProductName,
            item.Category,
            item.Price,
            item.StockQuantity,
            item.ReservedQuantity,
            AvailableQuantity = item.AvailableQuantity,
            item.LastUpdated
        });
    }

    [HttpGet("reservations")]
    public async Task<ActionResult> GetReservations([FromQuery] int? orderId = null)
    {
        _logger.LogInformation("GetReservations endpoint called - OrderId: {OrderId}", orderId);

        var query = _context.InventoryReservations.AsQueryable();

        if (orderId.HasValue)
        {
            query = query.Where(r => r.OrderId == orderId.Value);
        }

        var reservations = await query
            .OrderByDescending(r => r.CreatedAt)
            .Take(100)
            .ToListAsync();

        return Ok(reservations);
    }

    [HttpGet("health")]
    public ActionResult Health()
    {
        return Ok(new
        {
            Service = "InventoryService",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }
}
