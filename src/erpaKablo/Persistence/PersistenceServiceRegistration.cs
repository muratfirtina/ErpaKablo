using Application.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
using Persistence.DbConfiguration;
using Persistence.Repositories;

namespace Persistence;

public static class PersistenceServiceRegistration
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        services.AddDbContext<ErpaKabloDbContext>(opt=>opt.UseMySQL(Configuration.ConnectionString));
        
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductFeatureRepository, ProductFeatureRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<ICategoryFilterRepository, CategoryFilterRepository>();
        services.AddScoped<IEndpointRepository, EndpointRepository>();
        services.AddScoped<IACMenuRepository, ACMenuRepository>();
        
        return services;
    }
}