using Microsoft.EntityFrameworkCore;
using SportsStore.InventoryService.Data;
using SportsStore.InventoryService.Models;
using Xunit;

namespace SportsStore.InventoryService.Tests;

public class InventoryServiceTests
{
    private InventoryDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        var context = new InventoryDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public void CanCreateInventoryItem()
    {
   
        using var context = GetInMemoryContext();
        var item = new InventoryItem
        {
            ProductId = 1,
            ProductName = "Test Product",
            Category = "Test Category",
            Price = 99.99m,
            StockQuantity = 100,
            ReservedQuantity = 0
        };

    
        context.InventoryItems.Add(item);
        context.SaveChanges();

    
        Assert.Single(context.InventoryItems);
        Assert.Equal(100, context.InventoryItems.First().AvailableQuantity);
    }

    [Fact]
    public void CanReserveInventory()
    {
      
        using var context = GetInMemoryContext();
        var item = new InventoryItem
        {
            ProductId = 1,
            ProductName = "Test Product",
            Category = "Test",
            Price = 50m,
            StockQuantity = 100,
            ReservedQuantity = 0
        };
        context.InventoryItems.Add(item);
        context.SaveChanges();

   
        item.ReservedQuantity = 10;
        context.SaveChanges();

       
        var savedItem = context.InventoryItems.First();
        Assert.Equal(90, savedItem.AvailableQuantity);
    }

    [Fact]
    public void CanCreateReservation()
    {
       
        using var context = GetInMemoryContext();
        var item = new InventoryItem
        {
            ProductId = 1,
            ProductName = "Test Product",
            Category = "Test",
            Price = 50m,
            StockQuantity = 100,
            ReservedQuantity = 0
        };
        context.InventoryItems.Add(item);
        context.SaveChanges();


        var reservation = new InventoryReservation
        {
            OrderId = 1,
            ProductId = 1,
            ProductName = "Test Product",
            Quantity = 5,
            Status = "Reserved",
            CorrelationId = Guid.NewGuid()
        };
        context.InventoryReservations.Add(reservation);
        context.SaveChanges();

 
        Assert.Single(context.InventoryReservations);
        Assert.Equal("Reserved", context.InventoryReservations.First().Status);
    }

    [Fact]
    public void AvailableQuantity_CalculatedCorrectly()
    {
       
        var item = new InventoryItem
        {
            ProductId = 1,
            ProductName = "Test",
            Category = "Test",
            Price = 10m,
            StockQuantity = 50,
            ReservedQuantity = 15
        };


        Assert.Equal(35, item.AvailableQuantity);
    }
}
