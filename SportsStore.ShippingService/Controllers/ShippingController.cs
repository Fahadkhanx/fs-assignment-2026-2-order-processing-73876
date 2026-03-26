using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStore.ShippingService.Data;

namespace SportsStore.ShippingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShippingController : ControllerBase
{
    private readonly ShippingDbContext _context;
    private readonly ILogger<ShippingController> _logger;

    public ShippingController(
        ShippingDbContext context,
        ILogger<ShippingController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("shipments")]
    public async Task<ActionResult> GetShipments(
        [FromQuery] int? orderId = null,
        [FromQuery] string? status = null)
    {
        _logger.LogInformation("GetShipments endpoint called - OrderId: {OrderId}, Status: {Status}", orderId, status);

        var query = _context.Shipments.AsQueryable();

        if (orderId.HasValue)
        {
            query = query.Where(s => s.OrderId == orderId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(s => s.Status == status);
        }

        var shipments = await query
            .OrderByDescending(s => s.CreatedAt)
            .Take(100)
            .ToListAsync();

        return Ok(shipments);
    }

    [HttpGet("shipments/{shipmentId}")]
    public async Task<ActionResult> GetShipment(int shipmentId)
    {
        _logger.LogInformation("GetShipment endpoint called - ShipmentId: {ShipmentId}", shipmentId);

        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId);

        if (shipment == null)
            return NotFound();

        return Ok(shipment);
    }

    [HttpGet("carriers")]
    public async Task<ActionResult> GetCarriers()
    {
        _logger.LogInformation("GetCarriers endpoint called");

        var carriers = await _context.ShippingCarriers
            .Where(c => c.IsActive)
            .ToListAsync();

        return Ok(carriers);
    }

    [HttpGet("track/{trackingNumber}")]
    public async Task<ActionResult> TrackShipment(string trackingNumber)
    {
        _logger.LogInformation("TrackShipment endpoint called - TrackingNumber: {TrackingNumber}", trackingNumber);

        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);

        if (shipment == null)
            return NotFound("Shipment not found");

        return Ok(new
        {
            shipment.TrackingNumber,
            shipment.Carrier,
            shipment.Status,
            shipment.EstimatedDispatchDate,
            shipment.EstimatedDeliveryDate,
            shipment.ActualDispatchDate,
            shipment.ActualDeliveryDate
        });
    }

    [HttpGet("health")]
    public ActionResult Health()
    {
        return Ok(new
        {
            Service = "ShippingService",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }
}
