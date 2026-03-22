using SportsStore.Shared.Enums;

namespace SportsStore.Shared.DTOs;

public class OrderDto
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }

    // Shipping Address
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }

    public bool GiftWrap { get; set; }
    public string? StripeSessionId { get; set; }
}

public class CreateOrderDto
{
    public int CustomerId { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
    public bool GiftWrap { get; set; }
}

public class CheckoutDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? Line3 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Country { get; set; }
    public bool GiftWrap { get; set; }
    public List<CartItemDto> CartItems { get; set; } = new();
}

public class CartItemDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
}

public class OrderStatusDto
{
    public int OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public List<OrderEventDto> Events { get; set; } = new();
}

public class OrderEventDto
{
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Message { get; set; }
    public bool IsSuccess { get; set; }
}
