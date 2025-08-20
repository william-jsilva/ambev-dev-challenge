using Ambev.DeveloperEvaluation.ORM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.Integration;

public class DatabaseFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }

    public DatabaseFixture()
    {
        var services = new ServiceCollection();

        // Configurar banco de dados em memória
        services.AddDbContext<DefaultContext>(options =>
        {
            //options.UseInMemoryDatabase($"SaleTestDatabase_{Guid.NewGuid()}");
        });

        ServiceProvider = services.BuildServiceProvider();

        // Criar o banco de dados
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
