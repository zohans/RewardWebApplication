using Reward.Data;
using Reward.Models;
using System.Globalization;

namespace Reward.Data
{
    public static class DbInitializer
    {
        public static void Initialize(RewardDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any ProductModels.
            if (context.ProductModels.Any())
            {
                return;   // DB has been seeded
            }

            var productModels = new ProductModel[]
            {
            new ProductModel{ProductId = "PRD01", ProductName = "Vortex 95", Category = "Fuel", UnitPrice = 1.2M },
            new ProductModel{ProductId = "PRD02", ProductName = "Vortex 98", Category = "Fuel", UnitPrice = 1.3M },
            new ProductModel{ProductId = "PRD03", ProductName = "Diesel", Category = "Fuel", UnitPrice = 1.1M },
            new ProductModel{ProductId = "PRD04", ProductName = "Twix 55g", Category = "Shop", UnitPrice = 2.3M },
            new ProductModel{ProductId = "PRD05", ProductName = "Mars 72g", Category = "Shop", UnitPrice = 5.1M },
            new ProductModel{ProductId = "PRD06", ProductName = "SNICKERS 72G", Category = "Shop", UnitPrice = 3.4M },
            new ProductModel{ProductId = "PRD07", ProductName = "Bounty 3 63g", Category = "Shop", UnitPrice = 6.9M },
            new ProductModel{ ProductId = "PRD08", ProductName = "Snickers 50g", Category = "Shop", UnitPrice = 4.0M }
            };
            foreach (ProductModel s in productModels)
            {
                context.ProductModels.Add(s);
            }
            context.SaveChanges();


            var pointsPromotions = new PointsPromotion[]
            {
            new PointsPromotion{Id = "PP001", Name = "New Year Promo", StartDate = ParseDate("01-Jan-2020"), EndDate = ParseDate("30-Jan-2020"), Category = "Any", PointsPerDollar = 2 },
            new PointsPromotion{Id = "PP002", Name = "Fuel Promo", StartDate = ParseDate("05-Feb-2020"), EndDate = ParseDate("15-Feb-2020"), Category = "Fuel", PointsPerDollar = 3 },
            new PointsPromotion{Id = "PP003", Name = "Shop Promo", StartDate = ParseDate("01-Mar-2020"), EndDate = ParseDate("20-Mar-2020"), Category = "Shop", PointsPerDollar = 4 }
            };

            foreach (PointsPromotion e in pointsPromotions)
            {
                context.PointsPromotions.Add(e);
            }
            context.SaveChanges();

            var discountPromotions = new DiscountPromotion[]
            {
                new DiscountPromotion { Id = "DP001", Name = "Fuel Discount Promo", StartDate = ParseDate("01-Jan-2020"), EndDate = ParseDate("15-Feb-2020"), DiscountPercent = 0.20M, EligibleProductIds = new List<string> { "PRD02" } },
                new DiscountPromotion { Id = "DP002", Name = "Happy Promo", StartDate = ParseDate("02-Mar-2020"), EndDate = ParseDate("20-Mar-2020"), DiscountPercent = 0.15M, EligibleProductIds = new List<string>() }
            };

            foreach (DiscountPromotion e in discountPromotions)
            {
                context.DiscountPromotions.Add(e);
            }
            context.SaveChanges();

        }


        private static readonly CultureInfo Culture = new CultureInfo("en-US");

        // Helper to parse dates from the format 'DD-MMM-YYYY'
        private static DateTime ParseDate(string dateString) =>
            DateTime.ParseExact(dateString, "dd-MMM-yyyy", Culture);
    }
}
