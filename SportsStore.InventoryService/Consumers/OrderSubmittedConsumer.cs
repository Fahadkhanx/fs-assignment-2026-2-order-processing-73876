using MassTransit;
using SportsStore.Shared.Messages;

namespace SportsStore.InventoryService.Consumers;

public class OrderSubmittedConsumer : IConsumer<OrderSubmittedEvent>
{
    private readonly ILogger<OrderSubmittedConsumer> _logger;

    public OrderSubmittedConsumer(ILogger<OrderSubmittedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderSubmittedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Inventory Service: Processing OrderSubmittedEvent - OrderId: {OrderId}, CustomerId: {CustomerId}, CorrelationId: {CorrelationId}",
            message.OrderId, message.CustomerId, message.CorrelationId);

        // Simulate inventory check
        await Task.Delay(100); // Simulate processing time

        // Simulate inventory availability (90% success rate)
        var random = new Random();
        bool allItemsAvailable = random.Next(100) < 90;

        if (allItemsAvailable)
        {
            _logger.LogInformation(
                "Inventory Service: All items available for OrderId: {OrderId}, publishing InventoryConfirmedEvent",
                message.OrderId);

            var confirmedEvent = new InventoryConfirmedEvent
            {
                CorrelationId = message.CorrelationId,
                OrderId = message.OrderId,
                CustomerId = message.CustomerId,
                ReservedItems = message.Items.Select(i => new InventoryReservationItem
                {
                    ProductId = i.ProductId,
                    ReservedQuantity = i.Quantity
                }).ToList()
            };

            await context.Publish(confirmedEvent);
        }
        else
        {
            _logger.LogWarning(
                "Inventory Service: Insufficient stock for OrderId: {OrderId}, publishing InventoryFailedEvent",
                message.OrderId);

            var failedEvent = new InventoryFailedEvent
            {
                CorrelationId = message.CorrelationId,
                OrderId = message.OrderId,
                CustomerId = message.CustomerId,
                FailedItems = message.Items.Select(i => new InventoryFailureItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    RequestedQuantity = i.Quantity,
                    AvailableQuantity = Math.Max(0, i.Quantity - random.Next(1, 5))
                }).ToList(),
                FailureReason = "Insufficient stock for one or more items"
            };

            await context.Publish(failedEvent);
        }
    }
}
