using Ambev.DeveloperEvaluation.ORM;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Functional;

public class WebApplicationFactory : Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Ambev.DeveloperEvaluation.WebApi.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DefaultContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<DefaultContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });

            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<DefaultContext>();
                var logger = scopedServices.GetRequiredService<ILogger<WebApplicationFactory>>();

                db.Database.EnsureCreated();

                try
                {
                    // Seed test data if needed
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the database. Error: {Message}", ex.Message);
                }
            }
        });
    }
}
