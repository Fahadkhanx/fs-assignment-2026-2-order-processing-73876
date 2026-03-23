using MassTransit;
using SportsStore.Shared.Messages;

namespace SportsStore.ShippingService.Consumers;

public class PaymentApprovedConsumer : IConsumer<PaymentApprovedEvent>
{
    private readonly ILogger<PaymentApprovedConsumer> _logger;

    public PaymentApprovedConsumer(ILogger<PaymentApprovedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentApprovedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Shipping Service: Processing PaymentApprovedEvent - OrderId: {OrderId}, CustomerId: {CustomerId}, CorrelationId: {CorrelationId}",
            message.OrderId, message.CustomerId, message.CorrelationId);

        // Simulate shipment creation
        await Task.Delay(100); // Simulate processing time

        // Generate tracking number and shipment details
        var random = new Random();
        var trackingNumber = $"TRK{DateTime.UtcNow:yyyyMMdd}{random.Next(1000000, 9999999)}";
        var carriers = new[] { "FedEx", "UPS", "DHL", "USPS" };
        var carrier = carriers[random.Next(carriers.Length)];
        var estimatedDispatchDate = DateTime.UtcNow.AddDays(random.Next(1, 5));
        var shipmentId = random.Next(1000, 9999);

        _logger.LogInformation(
            "Shipping Service: Shipment created for OrderId: {OrderId}, ShipmentId: {ShipmentId}, TrackingNumber: {TrackingNumber}, Carrier: {Carrier}",
            message.OrderId, shipmentId, trackingNumber, carrier);

        var shippingCreatedEvent = new ShippingCreatedEvent
        {
            CorrelationId = message.CorrelationId,
            OrderId = message.OrderId,
            CustomerId = message.CustomerId,
            ShipmentId = shipmentId,
            TrackingNumber = trackingNumber,
            Carrier = carrier,
            EstimatedDispatchDate = estimatedDispatchDate
        };

        await context.Publish(shippingCreatedEvent);
    }
}
