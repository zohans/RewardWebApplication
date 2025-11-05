using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Reward.Data;

namespace Reward.Tests
{
    public static class ContextHelper
    {
        public static RewardDbContext GetRewardDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<RewardDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var context = new RewardDbContext(options);
            TestDataHelper.InitializeTestData(context);
            return context;
        }
    }
}
