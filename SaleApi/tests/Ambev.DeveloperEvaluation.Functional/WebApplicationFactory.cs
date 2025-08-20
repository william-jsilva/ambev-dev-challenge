using Ambev.DeveloperEvaluation.ORM;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Ambev.DeveloperEvaluation.WebApi;

namespace Ambev.DeveloperEvaluation.Functional;

public class WebApplicationFactory : WebApplicationFactory<Ambev.DeveloperEvaluation.WebApi.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DefaultContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Also remove the DefaultContext registration
            var contextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DefaultContext));

            if (contextDescriptor != null)
            {
                services.Remove(contextDescriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<DefaultContext>(options =>
            {
                options.UseInMemoryDatabase($"SaleTestDatabase_{Guid.NewGuid()}");
            });

            // Create a new service provider
            var serviceProvider = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<DefaultContext>();

            // Ensure the database is created
            db.Database.EnsureCreated();

            try
            {
                // Seed the database with test data if needed
                // SeedTestData(db);
            }
            catch (Exception ex)
            {
                // Log any errors that occur during seeding
                Console.WriteLine($"An error occurred seeding the database: {ex.Message}");
            }
        });
    }

    // Optional: Add test data seeding method
    private static void SeedTestData(DefaultContext context)
    {
        // Add any test data needed for functional tests
        // This method can be used to populate the database with known test data
    }
}
