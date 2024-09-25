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
    public DbSet<ProductFeatureValue> ProductFeatureValues { get; set; }
    public DbSet<ProductImageFile> ProductImageFiles { get; set; }
    public DbSet<CategoryImageFile> CategoryImageFiles { get; set; }
    public DbSet<BrandImageFile> BrandImageFiles { get; set; }
    public DbSet<CarouselImageFile> CarouselImageFiles { get; set; }
    public DbSet<ImageFile> ImageFiles { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<CompletedOrder> CompletedOrders { get; set; }
    public DbSet<Carousel> Carousel { get; set; }
    public DbSet<FilterGroup> FilterGroups { get; set; }
    public DbSet<FilterOption> FilterOptions { get; set; }
    public DbSet<ProductLike> ProductLikes { get; set; }
    public DbSet<ProductView> ProductViews { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }
    public DbSet<PhoneNumber> PhoneNumbers { get; set; }
    
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Product>().HasQueryFilter(p=>!p.DeletedDate.HasValue);
        builder.Entity<Brand>().HasQueryFilter(b => !b.DeletedDate.HasValue);
        builder.Entity<Category>().HasQueryFilter(c => !c.DeletedDate.HasValue);
        builder.Entity<Feature>().HasQueryFilter(f => !f.DeletedDate.HasValue);
        builder.Entity<ImageFile>().HasQueryFilter(i => !i.DeletedDate.HasValue);
        builder.Entity<FeatureValue>().HasQueryFilter(fv => !fv.DeletedDate.HasValue);
        builder.Entity<ProductFeatureValue>().HasQueryFilter(pfv => !pfv.Product.DeletedDate.HasValue);
        builder.Entity<Cart>().HasQueryFilter(c => !c.DeletedDate.HasValue);
        builder.Entity<CartItem>().HasQueryFilter(ci => !ci.DeletedDate.HasValue);
        builder.Entity<Order>().HasQueryFilter(o => !o.DeletedDate.HasValue);
        builder.Entity<OrderItem>().HasQueryFilter(oi => !oi.DeletedDate.HasValue);
        builder.Entity<CompletedOrder>().HasQueryFilter(co => !co.DeletedDate.HasValue);
        builder.Entity<Carousel>().HasQueryFilter(c => !c.DeletedDate.HasValue);
        builder.Entity<FilterGroup>().HasQueryFilter(fg => !fg.DeletedDate.HasValue);
        builder.Entity<FilterOption>().HasQueryFilter(fo => !fo.DeletedDate.HasValue);
        builder.Entity<ProductLike>().HasQueryFilter(pl => !pl.DeletedDate.HasValue);
        builder.Entity<ProductView>().HasQueryFilter(pv => !pv.DeletedDate.HasValue);
        builder.Entity<UserAddress>().HasQueryFilter(ua => !ua.DeletedDate.HasValue);
        builder.Entity<PhoneNumber>().HasQueryFilter(pn => !pn.DeletedDate.HasValue);
        
        
        builder.Entity<Order>()
            .HasIndex(o=>o.OrderCode)
            .IsUnique();
        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId);
        
        builder.Entity<Order>()
            .HasOne(o => o.CompletedOrder)
            .WithOne(c => c.Order)
            .HasForeignKey<CompletedOrder>(c => c.OrderId);
        
        builder.Entity<UserAddress>()
            .HasOne(ua => ua.User)
            .WithMany(u => u.UserAddresses)
            .HasForeignKey(ua => ua.UserId);
            
        builder.Entity<PhoneNumber>()
            .HasOne(pn => pn.User)
            .WithMany(u => u.PhoneNumbers)
            .HasForeignKey(pn => pn.UserId);
        
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
            .HasMany(c => c.Features)
            .WithMany(f => f.Categories)
            .UsingEntity(j => j.ToTable("CategoryFeature"));
        
        builder.Entity<Feature>()
            .HasMany(f => f.Categories)
            .WithMany(c => c.Features)
            .UsingEntity(j => j.ToTable("CategoryFeature"));
        
        builder.Entity<Feature>()
            .HasMany(f => f.FeatureValues)
            .WithOne(v => v.Feature)
            .HasForeignKey(v => v.FeatureId);
        
        builder.Entity<ProductFeatureValue>()
            .HasKey(pfv => new { pfv.ProductId, pfv.FeatureValueId });
        
        builder.Entity<ProductFeatureValue>()
            .HasOne(pfv => pfv.Product)
            .WithMany(p => p.ProductFeatureValues)
            .HasForeignKey(pfv => pfv.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        
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