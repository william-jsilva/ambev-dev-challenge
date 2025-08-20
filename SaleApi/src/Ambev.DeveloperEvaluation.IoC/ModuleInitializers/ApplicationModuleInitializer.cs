using Ambev.DeveloperEvaluation.Application.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

public class ApplicationModuleInitializer : IModuleInitializer
{
    public void Initialize(WebApplicationBuilder builder)
    {
        // Register event publisher with logging implementation for now
        builder.Services.AddScoped<IEventPublisher, LoggingEventPublisher>();
    }
}