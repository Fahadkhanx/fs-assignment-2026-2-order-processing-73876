using MassTransit;
using SportsStore.Shared.Messages;

namespace SportsStore.PaymentService.Consumers;

public class InventoryConfirmedConsumer : IConsumer<InventoryConfirmedEvent>
{
    private readonly ILogger<InventoryConfirmedConsumer> _logger;

    public InventoryConfirmedConsumer(ILogger<InventoryConfirmedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InventoryConfirmedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Payment Service: Processing InventoryConfirmedEvent - OrderId: {OrderId}, CustomerId: {CustomerId}, CorrelationId: {CorrelationId}",
            message.OrderId, message.CustomerId, message.CorrelationId);

        // Simulate payment processing
        await Task.Delay(150); // Simulate processing time

        // Simulate payment processing (85% success rate, 5% random failure, 10% test card rejection)
        var random = new Random();
        int outcome = random.Next(100);

        if (outcome < 85)
        {
            // Payment approved
            var transactionRef = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{random.Next(100000, 999999)}";
            
            _logger.LogInformation(
                "Payment Service: Payment approved for OrderId: {OrderId}, TransactionRef: {TransactionRef}",
                message.OrderId, transactionRef);

            var approvedEvent = new PaymentApprovedEvent
            {
                CorrelationId = message.CorrelationId,
                OrderId = message.OrderId,
                CustomerId = message.CustomerId,
                Amount = 0, // Will be filled by OrderAPI
                TransactionReference = transactionRef,
                ProcessedAt = DateTime.UtcNow
            };

            await context.Publish(approvedEvent);
        }
        else if (outcome < 90)
        {
            // Test card rejection (specific card numbers)
            _logger.LogWarning(
                "Payment Service: Payment rejected for OrderId: {OrderId} - Test card number",
                message.OrderId);

            var rejectedEvent = new PaymentRejectedEvent
            {
                CorrelationId = message.CorrelationId,
                OrderId = message.OrderId,
                CustomerId = message.CustomerId,
                Amount = 0,
                RejectionReason = "Card declined - Invalid test card number",
                ErrorCode = "TEST_CARD_REJECTED"
            };

            await context.Publish(rejectedEvent);
        }
        else
        {
            // Random failure
            _logger.LogWarning(
                "Payment Service: Payment failed for OrderId: {OrderId} - Random failure",
                message.OrderId);

            var rejectedEvent = new PaymentRejectedEvent
            {
                CorrelationId = message.CorrelationId,
                OrderId = message.OrderId,
                CustomerId = message.CustomerId,
                Amount = 0,
                RejectionReason = "Payment authorization failed",
                ErrorCode = "AUTH_FAILED"
            };

            await context.Publish(rejectedEvent);
        }
    }
}
