using MediatR;
using Microsoft.EntityFrameworkCore;
using SportsStore.OrderAPI.Data;
using SportsStore.Shared.Enums;

namespace SportsStore.OrderAPI.Queries;

public record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly OrderDbContext _context;

    public GetDashboardSummaryQueryHandler(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var orders = await _context.Orders.ToListAsync(cancellationToken);

        var summary = new DashboardSummaryDto
        {
            TotalOrders = orders.Count,
            CompletedOrders = orders.Count(o => o.Status == OrderStatus.Completed),
            FailedOrders = orders.Count(o => o.Status == OrderStatus.Failed ||
                                            o.Status == OrderStatus.InventoryFailed ||
                                            o.Status == OrderStatus.PaymentFailed),
            PendingOrders = orders.Count(o => o.Status == OrderStatus.Submitted ||
                                             o.Status == OrderStatus.InventoryPending ||
                                             o.Status == OrderStatus.PaymentPending ||
                                             o.Status == OrderStatus.ShippingPending),
            TotalRevenue = orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount),
            OrdersByStatus = orders.GroupBy(o => o.Status)
                                   .ToDictionary(g => g.Key.ToString(), g => g.Count())
        };

        return summary;
    }
}

public class DashboardSummaryDto
{
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int FailedOrders { get; set; }
    public int PendingOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public Dictionary<string, int> OrdersByStatus { get; set; } = new();
}
