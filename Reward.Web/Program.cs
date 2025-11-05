using Microsoft.EntityFrameworkCore;
using Reward.BusinessLogic;
using Reward.Data;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                // Ensures JSON keys with spaces (like "Transaction Date") can be mapped
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

        var configuration = builder.Configuration;
        builder.Services.AddDbContext<RewardDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("RewardContext")));

        // Register necessary services
        builder.Services.AddLazyCache();
        builder.Services.AddTransient<ICalculationService, CalculationService>();

        // Configure Swagger/OpenAPI
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // -----------------------------------------------------------------------
        // DATABASE INITIALIZATION (Your Requested Logic)
        // -----------------------------------------------------------------------
        InitializeDatabase(app);

        // -----------------------------------------------------------------------
        // MIDDLEWARE CONFIGURATION (Replaces Startup.Configure)
        // -----------------------------------------------------------------------
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();

        // Map controllers to endpoints
        app.MapControllers();

        app.Run();
    }

    /// <summary>
    /// Initializes the database context (e.g., creating the database and seeding data).
    /// </summary>
    private static void InitializeDatabase(IHost host)
    {
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                // --- Placeholder: Your Database Initialization Code ---
                var context = services.GetRequiredService<RewardDbContext>();
                DbInitializer.Initialize(context);
                // -----------------------------------------------------
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while creating or seeding the database.");
            }
        }
    }
}