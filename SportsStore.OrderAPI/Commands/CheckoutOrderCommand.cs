using AutoMapper;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SportsStore.OrderAPI.Data;
using SportsStore.OrderAPI.Models;
using SportsStore.Shared.DTOs;
using SportsStore.Shared.Enums;
using SportsStore.Shared.Messages;

namespace SportsStore.OrderAPI.Commands;

public record CheckoutOrderCommand(CheckoutDto Checkout) : IRequest<OrderDto>;

public class CheckoutOrderCommandHandler : IRequestHandler<CheckoutOrderCommand, OrderDto>
{
    private readonly OrderDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CheckoutOrderCommandHandler> _logger;

    public CheckoutOrderCommandHandler(
        OrderDbContext context,
        IMapper mapper,
        IPublishEndpoint publishEndpoint,
        ILogger<CheckoutOrderCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(CheckoutOrderCommand request, CancellationToken cancellationToken)
    {
        var checkout = request.Checkout;
        var correlationId = Guid.NewGuid();

        _logger.LogInformation(
            "Processing checkout - Customer: {CustomerName}, Email: {CustomerEmail}, CorrelationId: {CorrelationId}",
            checkout.Name, checkout.Email, correlationId);

        // Create or get customer
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == checkout.Email, cancellationToken);

        if (customer == null)
        {
            customer = new Customer
            {
                Name = checkout.Name ?? "Unknown",
                Email = checkout.Email ?? "unknown@example.com",
                Phone = checkout.Phone,
                Line1 = checkout.Line1,
                Line2 = checkout.Line2,
                Line3 = checkout.Line3,
                City = checkout.City,
                State = checkout.State,
                Zip = checkout.Zip,
                Country = checkout.Country,
                CreatedAt = DateTime.UtcNow
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created new customer - CustomerId: {CustomerId}", customer.CustomerId);
        }

        // Calculate total
        decimal totalAmount = checkout.CartItems.Sum(item => item.ProductPrice * item.Quantity);

        // Create order
        var order = new Order
        {
            CustomerId = customer.CustomerId,
            Status = OrderStatus.Submitted,
            TotalAmount = totalAmount,
            Line1 = checkout.Line1,
            Line2 = checkout.Line2,
            Line3 = checkout.Line3,
            City = checkout.City,
            State = checkout.State,
            Zip = checkout.Zip,
            Country = checkout.Country,
            GiftWrap = checkout.GiftWrap,
            CreatedAt = DateTime.UtcNow,
            Items = checkout.CartItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ProductPrice = item.ProductPrice,
                Quantity = item.Quantity
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Order created - OrderId: {OrderId}, CustomerId: {CustomerId}, TotalAmount: {TotalAmount}",
            order.OrderId, order.CustomerId, order.TotalAmount);

        // Publish OrderSubmittedEvent
        var orderSubmittedEvent = new OrderSubmittedEvent
        {
            CorrelationId = correlationId,
            OrderId = order.OrderId,
            CustomerId = customer.CustomerId,
            CustomerName = customer.Name,
            CustomerEmail = customer.Email,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(i => new OrderItemMessage
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductPrice = i.ProductPrice,
                Quantity = i.Quantity
            }).ToList(),
            Line1 = order.Line1,
            Line2 = order.Line2,
            Line3 = order.Line3,
            City = order.City,
            State = order.State,
            Zip = order.Zip,
            Country = order.Country
        };

        await _publishEndpoint.Publish(orderSubmittedEvent, cancellationToken);

        _logger.LogInformation(
            "OrderSubmittedEvent published - OrderId: {OrderId}, CorrelationId: {CorrelationId}",
            order.OrderId, correlationId);

        // Update order status
        order.Status = OrderStatus.InventoryPending;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        // Return DTO
        var orderDto = _mapper.Map<OrderDto>(order);
        orderDto.CustomerName = customer.Name;
        return orderDto;
    }
}
