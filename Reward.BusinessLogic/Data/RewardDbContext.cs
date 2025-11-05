using Microsoft.EntityFrameworkCore;
using Reward.Models;


namespace Reward.Data
{
    public class RewardDbContext : DbContext
    {
        public RewardDbContext(DbContextOptions<RewardDbContext> options) : base(options)
        {
        }

        public DbSet<ProductModel> ProductModels { get; set; }

        public DbSet<PointsPromotion> PointsPromotions { get; set; }

        public DbSet<DiscountPromotion> DiscountPromotions { get; set; }

    }
}
