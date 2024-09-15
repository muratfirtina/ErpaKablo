using Application.Features.ProductImageFiles.Dtos;
using Application.Features.Products.Dtos;
using Application.Repositories;
using Application.Storage;
using Core.Application.Requests;
using Core.Persistence.Paging;
using Core.Persistence.Repositories;
using Domain;
using Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class ProductRepository : EfRepositoryBase<Product, string, ErpaKabloDbContext>, IProductRepository
{
    public ProductRepository(ErpaKabloDbContext context) : base(context)
    {
    }

    public async Task<List<ProductImageFileDto>> GetFilesByProductId(string productId)
    {
        var query = Context.Products
            .Where(p => p.Id == productId)
            .SelectMany(p => p.ProductImageFiles)
            .OrderByDescending(e => e.CreatedDate)
            .Select(pif => new ProductImageFileDto
            {
                Id = pif.Id,
                Path = pif.Path,
                FileName = pif.Name,
                Showcase = pif.Showcase,
                Storage = pif.Storage,
                EntityType = pif.EntityType,
                Alt = pif.Alt,
            }).ToListAsync();

        return await query;
    }

    public async Task ChangeShowcase(string productId, string imageFileId, bool showcase)
    {
        //burada seçili product id de ki hangi productimagefile seçiliyse onun showcase değeri true il değiştirilecek diğerleri false olacak.
        var product = await Context.Products.FindAsync(int.Parse(productId));
        var productImageFile = await Context.ProductImageFiles
            .Where(pif => pif.Id == imageFileId)
            .FirstOrDefaultAsync();
        if (productImageFile != null)
        {
            productImageFile.Showcase = showcase;
            await Context.SaveChangesAsync();
        }
    }

    public async Task<ProductImageFile?> GetProductImage(string productId)
    {
        //burada gelen product id ye göre productimagefile tablosundan product id ye göre productimagefile getirilecek.
        var query = Context.Products
            .Where(p => p.Id == productId)
            .SelectMany(p => p.ProductImageFiles)
            .FirstOrDefaultAsync();

        return await query;
    }

    public async Task<IPaginate<Product>> SearchProductsAsync(string searchTerm, int pageIndex, int pageSize)
    {
        var query = Context.Products
            .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm) ||
                        p.Title.Contains(searchTerm))
            .Include(p => p.Brand).Include(p => p.Category).Include(p => p.ProductImageFiles)
            .Include(p => p.ProductFeatureValues).ThenInclude(p => p.FeatureValue).ThenInclude(p => p.Feature)
            .OrderByDescending(p => p.CreatedDate)
            .AsQueryable();

        return await query.ToPaginateAsync(pageIndex, pageSize);
    }

public async Task<IPaginate<Product>> FilterProductsAsync(string searchTerm, Dictionary<string, List<string>> filters, PageRequest pageRequest)
{
    var query = Context.Products
        .Include(p => p.Brand)
        .Include(p => p.Category)
        .Include(p => p.ProductImageFiles)
        .Include(p => p.ProductFeatureValues)
            .ThenInclude(pfv => pfv.FeatureValue)
                .ThenInclude(fv => fv.Feature)
        .AsQueryable();

    if (!string.IsNullOrEmpty(searchTerm))
    {
        query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm) || p.Title.Contains(searchTerm));
    }

    foreach (var filter in filters)
    {
        switch (filter.Key)
        {
            case "Brand":
                query = query.Where(p => filter.Value.Contains(p.Brand.Name));
                break;
            case "Price":
                if (filter.Value.Count > 0 && !string.IsNullOrWhiteSpace(filter.Value[0]))
                {
                    var priceRange = filter.Value[0].Split('-');
                    if (priceRange.Length == 2)
                    {
                        if (decimal.TryParse(priceRange[0], out decimal minPrice) && minPrice > 0)
                        {
                            query = query.Where(p => p.Price >= minPrice);
                        }

                        if (decimal.TryParse(priceRange[1], out decimal maxPrice) && maxPrice > 0)
                        {
                            query = query.Where(p => p.Price <= maxPrice);
                        }
                    }
                }
                break;
            default:
                query = query.Where(p => p.ProductFeatureValues.Any(pfv => 
                    pfv.FeatureValue.Feature.Name == filter.Key && 
                    filter.Value.Contains(pfv.FeatureValue.Name)));
                break;
        }
    }

    query = query.OrderByDescending(p => p.CreatedDate);

    return await query.ToPaginateAsync(pageRequest.PageIndex, pageRequest.PageSize);
}
    public async Task<List<FilterDefinition>> GetAvailableFilters(string searchTerm = null)
    {
        var filterDefinitions = new List<FilterDefinition>();

        IQueryable<Product> productsQuery = Context.Products;

        // Eğer bir arama terimi varsa, önce bu terime göre ürünleri filtrele
        if (!string.IsNullOrEmpty(searchTerm))
        {
            productsQuery = productsQuery.Where(p =>
                p.Name.Contains(searchTerm) ||
                p.Description.Contains(searchTerm) ||
                p.Title.Contains(searchTerm));
        }

        // Filtrelenmiş ürünleri al
        var products = await productsQuery
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.ProductFeatureValues)
            .ThenInclude(pfv => pfv.FeatureValue)
            .ThenInclude(fv => fv.Feature)
            .ToListAsync();

        // Markalar
        var brands = products.Select(p => p.Brand.Name).Distinct().ToList();
        filterDefinitions.Add(new FilterDefinition
        {
            Key = "Brand",
            DisplayName = "Marka",
            Type = FilterType.Checkbox,
            Options = brands
        });

        // Kategoriler
        /*var categories = products.Select(p => p.Category.Name).Distinct().ToList();
        filterDefinitions.Add(new FilterDefinition
        {
            Key = "Category",
            DisplayName = "Kategori",
            Type = FilterType.Checkbox,
            Options = categories
        });*/

        // Özellikler
        var features = products
            .SelectMany(p => p.ProductFeatureValues)
            .GroupBy(pfv => pfv.FeatureValue.Feature.Name)
            .ToDictionary(
                g => g.Key,
                g => g.Select(pfv => pfv.FeatureValue.Name).Distinct().ToList()
            );

        foreach (var feature in features)
        {
            filterDefinitions.Add(new FilterDefinition
            {
                Key = feature.Key,
                DisplayName = feature.Key,
                Type = FilterType.Checkbox,
                Options = feature.Value
            });
        }

        // Fiyat aralığı
        var minPrice = (decimal)products.Min(p => p.Price);
        var maxPrice = (decimal)products.Max(p => p.Price);
        
        //max price için arama teriminden gelen ürünlerin arasında ki en yüksek fiyatı bul.
        if (!string.IsNullOrEmpty(searchTerm))
        {
            var maxPriceFromSearch = (decimal)productsQuery.Max(p => p.Price);
            maxPrice = Math.Max(maxPrice, maxPriceFromSearch);
        }
        
        
        var priceRanges = GeneratePriceRanges(minPrice, maxPrice, 7);

        var priceFilterGroup = new FilterDefinition
        {
            Key = "Price",
            DisplayName = "Fiyat",
            Type = FilterType.Range,
            Options = priceRanges.Select(r => $"{r.Item1}-{r.Item2}").ToList()
            
        };
        filterDefinitions.Add(priceFilterGroup);

        return filterDefinitions;
    }
    
    private List<(decimal, decimal)> GeneratePriceRanges(decimal minPrice, decimal maxPrice, int steps)
    {
        var ranges = new List<(decimal, decimal)>();
        var step = (maxPrice - minPrice) / steps;

        for (int i = 0; i < steps; i++)
        {
            var start = minPrice + (step * i);
            var end = (i == steps - 1) ? maxPrice : minPrice + (step * (i + 1));
        
            // Başlangıç ve bitiş değerlerini yuvarla
            start = Math.Floor(start);
            end = Math.Ceiling(end);

            ranges.Add((start, end));
        }

        return ranges;
    }
}