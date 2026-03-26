using SportsStore.Blazor.DTOs;
using Microsoft.JSInterop;

namespace SportsStore.Blazor.Services;

public class CartService : ICartService
{
    private readonly ILocalStorageService _localStorage;
    private CartDto _cart = new();
    public event Action? OnCartChanged;

    public CartService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<CartDto> GetCartAsync()
    {
        var cart = await _localStorage.GetItemAsync<CartDto>("cart");
        _cart = cart ?? new CartDto();
        return _cart;
    }

    public async Task AddToCartAsync(ProductDto product, int quantity = 1)
    {
        await GetCartAsync();

        var existingItem = _cart.Items.FirstOrDefault(i => i.ProductId == product.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            _cart.Items.Add(new CartItemDto
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity
            });
        }

        await SaveCartAsync();
    }

    public async Task RemoveFromCartAsync(long productId)
    {
        await GetCartAsync();
        _cart.Items.RemoveAll(i => i.ProductId == productId);
        await SaveCartAsync();
    }

    public async Task UpdateQuantityAsync(long productId, int quantity)
    {
        await GetCartAsync();
        var item = _cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                _cart.Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
        }
        await SaveCartAsync();
    }

    public async Task ClearCartAsync()
    {
        _cart = new CartDto();
        await SaveCartAsync();
    }

    private async Task SaveCartAsync()
    {
        await _localStorage.SetItemAsync("cart", _cart);
        OnCartChanged?.Invoke();
    }
}

// Simple localStorage service interface
public interface ILocalStorageService
{
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);
}

public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
        if (string.IsNullOrEmpty(json))
            return default;
        return System.Text.Json.JsonSerializer.Deserialize<T>(json);
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(value);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
    }
}
