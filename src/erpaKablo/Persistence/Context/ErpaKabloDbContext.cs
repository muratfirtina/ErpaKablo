using Core.Persistence.Repositories;
using Domain;
using Domain.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using File = System.IO.File;

namespace Persistence.Context;

public class ErpaKabloDbContext : IdentityDbContext<AppUser,AppRole,string>
{
    public ErpaKabloDbContext(DbContextOptions<ErpaKabloDbContext> options) : base(options)
    {
        
    }

    public DbSet<Endpoint> Endpoints { get; set; }
    public DbSet<ACMenu> ACMenus { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<CategoryFilter> CategoryFilters { get; set; }
    public DbSet<ProductFeature> ProductFeatures { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<UIFilter> Filters { get; set; }
    public DbSet<ProductImageFile> ProductImageFiles { get; set; }
    public DbSet<ImageFile> ImageFiles { get; set; }
    
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<Product>()
            .HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<Category>()
            .HasKey(c => c.Id);
        
        builder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<CategoryFilter>()
            .HasKey(cf => new { cf.CategoryId, cf.FilterId });
        
        builder.Entity<CategoryFilter>()
            .HasOne(cf => cf.Category)
            .WithMany(c => c.CategoryFilters)
            .HasForeignKey(cf => cf.CategoryId);
        
        builder.Entity<CategoryFilter>()
            .HasOne(cf => cf.Filter)
            .WithMany(f => f.CategoryFilters)
            .HasForeignKey(cf => cf.FilterId);

        builder.Entity<ProductFeature>()
            .HasOne(p => p.Product)
            .WithMany(pf => pf.ProductFeatures)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        
        base.OnModelCreating(builder);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {   
        
        var datas = ChangeTracker.Entries<Entity<int>>();
        foreach (var data in datas)
        {
            _ = data.State switch
            {
                EntityState.Added => data.Entity.CreatedDate = DateTime.UtcNow,
                EntityState.Modified => data.Entity.UpdatedDate = DateTime.UtcNow,
                _ => DateTime.UtcNow
            };
        }
        
        return await base.SaveChangesAsync(cancellationToken);
    }
}