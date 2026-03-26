using System.Net.Http.Json;
using SportsStore.Blazor.DTOs;

namespace SportsStore.Blazor.Services;

public class OrderService : IOrderService
{
    private readonly HttpClient _httpClient;

    public OrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OrderDto> CheckoutAsync(CheckoutDto checkout)
    {
        var response = await _httpClient.PostAsJsonAsync("api/orders/checkout", checkout);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<OrderDto>();
        return result ?? throw new InvalidOperationException("Checkout failed");
    }

    public async Task<IEnumerable<OrderDto>> GetCustomerOrdersAsync(string email)
    {
        var result = await _httpClient.GetFromJsonAsync<IEnumerable<OrderDto>>($"api/orders/customer?email={Uri.EscapeDataString(email)}");
        return result ?? Enumerable.Empty<OrderDto>();
    }

    public async Task<OrderDto?> GetOrderAsync(int orderId)
    {
        return await _httpClient.GetFromJsonAsync<OrderDto>($"api/orders/{orderId}");
    }
}
