using MediatR;
using Microsoft.AspNetCore.Mvc;
using SportsStore.OrderAPI.Commands;
using SportsStore.OrderAPI.Queries;
using SportsStore.Shared.DTOs;
using SportsStore.Shared.Enums;

namespace SportsStore.OrderAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<OrderDto>> Checkout([FromBody] CheckoutDto checkout)
    {
        _logger.LogInformation("Checkout endpoint called - Customer: {CustomerName}", checkout.Name);

        try
        {
            var command = new CheckoutOrderCommand(checkout);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.OrderId }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Checkout failed");
            return StatusCode(500, "An error occurred during checkout");
        }
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<OrderDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("GetAll orders endpoint called - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var query = new GetOrdersQuery(page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        _logger.LogInformation("GetById endpoint called - OrderId: {OrderId}", id);

        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("{id}/status")]
    public async Task<ActionResult<OrderStatusDto>> GetStatus(int id)
    {
        _logger.LogInformation("GetStatus endpoint called - OrderId: {OrderId}", id);

        var query = new GetOrderByIdQuery(id);
        var order = await _mediator.Send(query);

        if (order == null)
            return NotFound();

        var statusDto = new OrderStatusDto
        {
            OrderId = order.OrderId,
            Status = order.Status,
            StatusMessage = GetStatusMessage(order.Status),
            LastUpdated = order.UpdatedAt ?? order.CreatedAt,
            Events = new List<OrderEventDto>()
        };

        return Ok(statusDto);
    }

    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<List<OrderDto>>> GetByStatus(OrderStatus status)
    {
        _logger.LogInformation("GetByStatus endpoint called - Status: {Status}", status);

        var query = new GetOrdersByStatusQuery(status);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<bool>> Cancel(int id, [FromBody] string reason)
    {
        _logger.LogInformation("Cancel endpoint called - OrderId: {OrderId}", id);

        var command = new CancelOrderCommand(id, reason ?? "Cancelled by user");
        var result = await _mediator.Send(command);

        if (!result)
            return BadRequest("Unable to cancel order");

        return Ok(result);
    }

    [HttpGet("customers/{customerId}/orders")]
    public async Task<ActionResult<List<OrderDto>>> GetCustomerOrders(int customerId)
    {
        _logger.LogInformation("GetCustomerOrders endpoint called - CustomerId: {CustomerId}", customerId);

        var query = new GetCustomerOrdersQuery(customerId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("dashboard/summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary()
    {
        _logger.LogInformation("GetDashboardSummary endpoint called");

        var query = new GetDashboardSummaryQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    private static string GetStatusMessage(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Cart => "Order is in cart",
            OrderStatus.Submitted => "Order has been submitted",
            OrderStatus.InventoryPending => "Checking inventory availability",
            OrderStatus.InventoryConfirmed => "Inventory confirmed",
            OrderStatus.InventoryFailed => "Inventory check failed",
            OrderStatus.PaymentPending => "Processing payment",
            OrderStatus.PaymentApproved => "Payment approved",
            OrderStatus.PaymentFailed => "Payment failed",
            OrderStatus.ShippingPending => "Preparing shipment",
            OrderStatus.ShippingCreated => "Shipment created",
            OrderStatus.Completed => "Order completed",
            OrderStatus.Failed => "Order failed",
            _ => "Unknown status"
        };
    }
}
