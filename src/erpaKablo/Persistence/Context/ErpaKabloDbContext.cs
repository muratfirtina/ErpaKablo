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
    public DbSet<Feature> Features { get; set; }
    public DbSet<FeatureValue> FeatureValues { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<VariantFeatureValue> VariantFeatureValues { get; set; }
    public DbSet<VariantGroup> VariantGroups { get; set; }
    public DbSet<CategoryFeature> CategoryFeatures { get; set; }
    public DbSet<ProductVariantGroup> ProductVariantGroups { get; set; }
    public DbSet<ProductImageFile> ProductImageFiles { get; set; }
    public DbSet<ImageFile> ImageFiles { get; set; }
    
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Product>().HasQueryFilter(p=>!p.DeletedDate.HasValue);
        builder.Entity<Brand>().HasQueryFilter(b => !b.DeletedDate.HasValue);
        builder.Entity<Category>().HasQueryFilter(c => !c.DeletedDate.HasValue);
        builder.Entity<Feature>().HasQueryFilter(f => !f.DeletedDate.HasValue);
        builder.Entity<ImageFile>().HasQueryFilter(i => !i.DeletedDate.HasValue);
        builder.Entity<FeatureValue>().HasQueryFilter(fv => !fv.DeletedDate.HasValue);
        builder.Entity<ProductVariant>().HasQueryFilter(pv => !pv.DeletedDate.HasValue);
        builder.Entity<VariantGroup>().HasQueryFilter(vg => !vg.DeletedDate.HasValue);
        builder.Entity<VariantFeatureValue>().HasQueryFilter(vfv => !vfv.Feature.DeletedDate.HasValue);
        builder.Entity<CategoryFeature>().HasQueryFilter(cf => !cf.Feature.DeletedDate.HasValue);
        builder.Entity<ProductVariantGroup>().HasQueryFilter(pvg => !pvg.VariantGroup.DeletedDate.HasValue);
        
        
        builder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<Product>()
            .HasMany(p => p.ProductVariants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId);
        
        builder.Entity<Product>()
            .HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<Category>()
            .HasKey(c => c.Id);
        
        builder.Entity<CategoryFeature>()
            .HasKey(cf => new { cf.CategoryId, cf.FeatureId });

        builder.Entity<CategoryFeature>()
            .HasOne(cf => cf.Category)
            .WithMany(c => c.CategoryFeatures)
            .HasForeignKey(cf => cf.CategoryId);

        builder.Entity<CategoryFeature>()
            .HasOne(cf => cf.Feature)
            .WithMany(f => f.CategoryFeatures)
            .HasForeignKey(cf => cf.FeatureId);
        
        builder.Entity<VariantFeatureValue>()
            .HasKey(vfv => new { vfv.ProductVariantId, vfv.FeatureId, vfv.FeatureValueId });
        
        builder.Entity<VariantFeatureValue>()
            .HasOne(vfv => vfv.ProductVariant)
            .WithMany(pv => pv.VariantFeatureValues)
            .HasForeignKey(vfv => vfv.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<ProductVariant>()
            .HasMany(v => v.VariantFeatureValues)
            .WithOne(f => f.ProductVariant)
            .HasForeignKey(f => f.ProductVariantId);
        
        builder.Entity<Feature>()
            .HasMany(f => f.FeatureValues)
            .WithOne(v => v.Feature)
            .HasForeignKey(v => v.FeatureId);
        
        /*builder.Entity<VariantFeatureValue>()
            .HasOne(vfv => vfv.FeatureValue)
            .WithMany(fv => fv.VariantFeatureValues)
            .HasForeignKey(vfv => vfv.FeatureValueId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<VariantFeatureValue>()
            .HasOne(vfv => vfv.Feature)
            .WithMany(f => f.VariantFeatureValues)
            .HasForeignKey(vfv => vfv.FeatureId)
            .OnDelete(DeleteBehavior.Restrict);*/
        
        builder.Entity<VariantFeatureValue>()
            .HasOne(vf => vf.Feature)
            .WithMany()
            .HasForeignKey(vf => vf.FeatureId);
        
        builder.Entity<ProductVariantGroup>()
            .HasKey(pvg => new { pvg.ProductId, pvg.VariantGroupId });
        
        builder.Entity<ProductVariantGroup>()
            .HasOne(pvg => pvg.Product)
            .WithMany()
            .HasForeignKey(pvg => pvg.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<ProductVariantGroup>()
            .HasOne(pvg => pvg.VariantGroup)
            .WithMany()
            .HasForeignKey(pvg => pvg.VariantGroupId)
            .OnDelete(DeleteBehavior.Restrict);
        
        
        /*builder.Entity<Feature>()
            .HasMany(f => f.FeatureValues)
            .WithOne(fv => fv.Feature)
            .HasForeignKey(fv => fv.FeatureId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<FeatureValue>()
            .HasOne(fv => fv.Feature)
            .WithMany(f => f.FeatureValues)
            .HasForeignKey(fv => fv.FeatureId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<ProductVariant>()
            .HasOne(pv => pv.Product)
            .WithMany(p => p.ProductVariants)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<ProductVariant>()
            .HasMany(pv => pv.ProductVariantFeatures)
            .WithOne(pvf => pvf.ProductVariant)
            .HasForeignKey(pvf => pvf.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProductVariantFeature>()
            .HasOne(pvf => pvf.ProductVariant)
            .WithMany(pv => pv.ProductVariantFeatures)
            .HasForeignKey(pvf => pvf.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<ProductVariantFeature>()
            .HasOne(pvf => pvf.FeatureValue)
            .WithMany(fv => fv.ProductVariantFeatures)
            .HasForeignKey(pvf => pvf.FeatureValueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProductVariantFeature>()
            .HasOne(pvf => pvf.Feature)
            .WithMany(f => f.ProductVariantFeatures)
            .HasForeignKey(pvf => pvf.FeatureId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<ProductImageFile>()
            .HasOne(pif => pif.Product)
            .WithMany(p => p.ProductImageFiles)
            .HasForeignKey(pif => pif.ProductId)
            .OnDelete(DeleteBehavior.Restrict);*/
        
        
        base.OnModelCreating(builder);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {   
        
        var datas = ChangeTracker.Entries<Entity<string>>();
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