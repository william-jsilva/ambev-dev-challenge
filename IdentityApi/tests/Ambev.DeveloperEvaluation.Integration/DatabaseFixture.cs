using Ambev.DeveloperEvaluation.ORM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.Integration;

public class DatabaseFixture : IDisposable
{
    private readonly DefaultContext _context;
    private readonly IServiceProvider _serviceProvider;

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DefaultContext(options);
        _context.Database.EnsureCreated();

        var services = new ServiceCollection();
        services.AddDbContext<DefaultContext>(options => options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        _serviceProvider = services.BuildServiceProvider();
    }

    public DefaultContext GetContext()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new DefaultContext(options);
    }

    public IServiceProvider GetServiceProvider()
    {
        return _serviceProvider;
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
