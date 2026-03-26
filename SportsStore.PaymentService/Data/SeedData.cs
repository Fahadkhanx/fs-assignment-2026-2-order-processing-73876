using SportsStore.PaymentService.Data;
using SportsStore.PaymentService.Models;

namespace SportsStore.PaymentService.Data;

public static class SeedData
{
    public static void EnsurePopulated(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

        if (!context.TestCards.Any())
        {
            context.TestCards.AddRange(
                // Success cards
                new TestCard { CardNumber = "4111111111111111", CardType = "Success", Description = "Visa - Always approves" },
                new TestCard { CardNumber = "5555555555554444", CardType = "Success", Description = "MasterCard - Always approves" },
                new TestCard { CardNumber = "378282246310005", CardType = "Success", Description = "Amex - Always approves" },

                // Decline cards
                new TestCard { CardNumber = "4000000000000002", CardType = "Decline", Description = "Visa - Always declines" },
                new TestCard { CardNumber = "5105105105105100", CardType = "Decline", Description = "MasterCard - Always declines" },

                // Error cards
                new TestCard { CardNumber = "4000000000000119", CardType = "Error", Description = "Visa - Processing error" },
                new TestCard { CardNumber = "4000000000003220", CardType = "Error", Description = "Visa - 3D Secure required" }
            );
            context.SaveChanges();
        }
    }
}
