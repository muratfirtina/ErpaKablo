using Application.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
using Persistence.DbConfiguration;
using Persistence.Repositories;

namespace Persistence;

public static class ServiceRegistration
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        services.AddDbContext<ErpaKabloDbContext>(opt=>opt.UseMySQL(Configuration.ConnectionString));
        
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IEndpointRepository, EndpointRepository>();
        services.AddScoped<IACMenuRepository, ACMenuRepository>();
        services.AddScoped<IImageFileRepository, ImageFileRepository>();
        services.AddScoped<IEndpointRepository, EndpointRepository>();
        services.AddScoped<IFeatureRepository, FeatureRepository>();
        services.AddScoped<IFeatureValueRepository, FeatureValueRepository>();
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        services.AddScoped<IVariantGroupRepository, VariantGroupRepository>();
        
        
        return services;
    }
}