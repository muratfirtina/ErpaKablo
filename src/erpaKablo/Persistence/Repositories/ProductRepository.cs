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
        var product = await Context.Products.FindAsync(productId);
        if (product == null) return;

        var productImageFiles = await Context.ProductImageFiles
            .Where(pif => pif.Id == productId)
            .ToListAsync();

        foreach (var pif in productImageFiles)
        {
            pif.Showcase = pif.Id == imageFileId && showcase;
        }

        await Context.SaveChangesAsync();
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
        var query = Context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var terms = searchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var term in terms)
            {
                var termParam = term.ToLower();
                query = query.Where(p => 
                    EF.Functions.Like(p.Name.ToLower(), $"%{termParam}%") ||
                    EF.Functions.Like(p.Description.ToLower(), $"%{termParam}%") ||
                    EF.Functions.Like(p.Title.ToLower(), $"%{termParam}%") ||
                    EF.Functions.Like(p.Brand.Name.ToLower(), $"%{termParam}%") ||
                    EF.Functions.Like(p.Category.Name.ToLower(), $"%{termParam}%") ||
                    p.ProductFeatureValues.Any(pfv => 
                        EF.Functions.Like(pfv.FeatureValue.Name.ToLower(), $"%{termParam}%"))
                
                );
            }
        }

        query = query
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.ProductImageFiles)
            .Include(p => p.ProductFeatureValues)
            .ThenInclude(p => p.FeatureValue)
            .ThenInclude(p => p.Feature)
            .OrderByDescending(p => p.CreatedDate);

        return await query.ToPaginateAsync(pageIndex, pageSize);
    }

    public async Task<IPaginate<Product>> FilterProductsAsync(string searchTerm, Dictionary<string, List<string>> filters, PageRequest pageRequest, string sortOrder)
{
    var query = Context.Products
        .Include(p => p.Brand)
        .Include(p => p.Category)
        .Include(p => p.ProductImageFiles)
        .Include(p => p.ProductFeatureValues)
            .ThenInclude(pfv => pfv.FeatureValue)
                .ThenInclude(fv => fv.Feature)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        var terms = searchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var term in terms)
        {
            var termParam = term.ToLower();
            query = query.Where(p => 
                EF.Functions.Like(p.Name.ToLower(), $"%{termParam}%") ||
                EF.Functions.Like(p.Description.ToLower(), $"%{termParam}%") ||
                EF.Functions.Like(p.Title.ToLower(), $"%{termParam}%") ||
                EF.Functions.Like(p.Brand.Name.ToLower(), $"%{termParam}%") ||
                EF.Functions.Like(p.Category.Name.ToLower(), $"%{termParam}%") ||
                p.ProductFeatureValues.Any(pfv => 
                    EF.Functions.Like(pfv.FeatureValue.Name.ToLower(), $"%{termParam}%"))
                
            );
        }
    }

    foreach (var filter in filters.Where(f => f.Value.Count > 0))
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
    
    switch (sortOrder)
    {
        case "price_asc":
            query = query.OrderBy(p => p.Price);
            break;
        case "price_desc":
            query = query.OrderByDescending(p => p.Price);
            break;
        default:
            query = query.OrderByDescending(p => p.CreatedDate);
            break;
    }

    var result = await query.ToPaginateAsync(pageRequest.PageIndex, pageRequest.PageSize);

    if (result.Items.Count == 0)
    {
        Console.WriteLine($"No results found for searchTerm: {searchTerm}, filters: {string.Join(", ", filters.Select(f => $"{f.Key}:[{string.Join(",", f.Value)}]"))}");
    }

    return result;
}

    public async Task<List<FilterGroup>> GetAvailableFilters(string searchTerm = null)
    {
        var filterDefinitions = new List<FilterGroup>();

        IQueryable<Product> productsQuery = Context.Products;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            productsQuery = productsQuery.Where(p =>
                EF.Functions.Like(p.Name, $"%{searchTerm}%") ||
                EF.Functions.Like(p.Description, $"%{searchTerm}%") ||
                EF.Functions.Like(p.Title, $"%{searchTerm}%"));
        }

        var products = await productsQuery
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.ProductFeatureValues)
            .ThenInclude(pfv => pfv.FeatureValue)
            .ThenInclude(fv => fv.Feature)
            .ToListAsync();

        var brands = products.Select(p => p.Brand.Name).Distinct().ToList();
        filterDefinitions.Add(new FilterGroup
        {
            Name = "Brand",
            DisplayName = "Marka",
            Type = FilterType.Checkbox,
            Options = brands.Select(b => new FilterOption
            {
                Value = b,
                DisplayValue = b
            }).ToList()
        });

        var features = products
            .SelectMany(p => p.ProductFeatureValues)
            .GroupBy(pfv => pfv.FeatureValue.Feature.Name)
            .ToDictionary(
                g => g.Key,
                g => g.Select(pfv => pfv.FeatureValue.Name).Distinct().ToList()
            );

        foreach (var feature in features)
        {
            filterDefinitions.Add(new FilterGroup
            {
                Name = feature.Key,
                DisplayName = feature.Key,
                Type = FilterType.Checkbox,
                Options = feature.Value.Select(f => new FilterOption
                {
                    Value = f,
                    DisplayValue = f
                }).ToList()
            });
        }

        var minPrice = products.Min(p => p.Price);
        var maxPrice = products.Max(p => p.Price);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var maxPriceFromSearch = productsQuery.Max(p => p.Price);
            maxPrice = Math.Max((decimal)maxPrice, (decimal)maxPriceFromSearch);
        }

        var priceRanges = GeneratePriceRanges(minPrice, maxPrice, 7);

        filterDefinitions.Add(new FilterGroup
        {
            Name = "Price",
            DisplayName = "Fiyat",
            Type = FilterType.Range,
            Options = priceRanges.Select(r => new FilterOption
            {
                Value = $"{r.Item1}-{r.Item2}",
                DisplayValue = $"{r.Item1:C0} - {r.Item2:C0}"
            }).ToList()
        });

        return filterDefinitions;
    }

    private List<(decimal? start, decimal? end)> GeneratePriceRanges(decimal? minPrice, decimal? maxPrice, int steps)
    {
        var ranges = new List<(decimal? start, decimal? end)>();
        var step = (maxPrice - minPrice) / steps;

        for (int i = 0; i < steps; i++)
        {
            var start = minPrice + (step * i);
            var end = (i == steps - 1) ? maxPrice : minPrice + (step * (i + 1));

            start = Math.Floor((decimal)start);
            end = Math.Ceiling((decimal)end);

            ranges.Add((start, end));
        }

        return ranges;
    }
}