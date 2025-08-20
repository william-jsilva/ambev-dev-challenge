using Ambev.DeveloperEvaluation.ORM.Extensions;
using Microsoft.AspNetCore.Builder;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

public class InfrastructureModuleInitializer : IModuleInitializer
{
    public void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddMongoInfrastructure(builder.Configuration);
        builder.Services.AddRedisCache(builder.Configuration);
    }
}