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
            var terms = searchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.ToLower())
                .ToList();

            // Önce tam eşleşmeleri ara
            var exactMatchQuery = query.Where(p =>
                terms.All(term =>
                    EF.Functions.Like(p.Name.ToLower(), $"%{term}%") ||
                    EF.Functions.Like(p.Brand.Name.ToLower(), $"%{term}%") ||
                    EF.Functions.Like(p.Category.Name.ToLower(), $"%{term}%")
                ));

            // Tam eşleşme yoksa diğer alanlarda da ara
            var fallbackQuery = query.Where(p =>
                terms.All(term =>
                    EF.Functions.Like(p.Name.ToLower(), $"%{term}%") ||
                    EF.Functions.Like(p.Description.ToLower(), $"%{term}%") ||
                    EF.Functions.Like(p.Title.ToLower(), $"%{term}%") ||
                    EF.Functions.Like(p.Brand.Name.ToLower(), $"%{term}%") ||
                    EF.Functions.Like(p.Category.Name.ToLower(), $"%{term}%") ||
                    p.ProductFeatureValues.Any(pfv =>
                        EF.Functions.Like(pfv.FeatureValue.Name.ToLower(), $"%{term}%"))
                ));

            // İki sorguyu birleştir
            query = exactMatchQuery.Union(fallbackQuery);
        }

        query = query
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.ProductImageFiles)
            .Include(p => p.ProductFeatureValues)
            .ThenInclude(p => p.FeatureValue)
            .ThenInclude(p => p.Feature)
            .AsSplitQuery() 
            .OrderByDescending(p => p.CreatedDate);

        return await query.ToPaginateAsync(pageIndex, pageSize);
    }

    public async Task<IPaginate<Product>> FilterProductsAsync(string searchTerm,
    Dictionary<string, List<string>> filters, PageRequest pageRequest, string sortOrder)
{
    var query = Context.Products
        .Include(p => p.Brand)
        .Include(p => p.Category)
        .Include(p => p.ProductImageFiles)
        .Include(p => p.ProductFeatureValues)
        .ThenInclude(pfv => pfv.FeatureValue)
        .ThenInclude(fv => fv.Feature)
        .AsSplitQuery() 
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        var terms = searchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.ToLower().Trim())
            .ToList();

        // Her terimin en az bir alanda bulunması gerekiyor
        foreach (var term in terms)
        {
            query = query.Where(p =>
                EF.Functions.Like(p.Name.ToLower(), $"%{term}%") ||
                EF.Functions.Like(p.Description.ToLower(), $"%{term}%") ||
                EF.Functions.Like(p.Title.ToLower(), $"%{term}%") ||
                EF.Functions.Like(p.Brand.Name.ToLower(), $"%{term}%") ||
                EF.Functions.Like(p.Category.Name.ToLower(), $"%{term}%") ||
                p.ProductFeatureValues.Any(pfv =>
                    EF.Functions.Like(pfv.FeatureValue.Name.ToLower(), $"%{term}%"))
            );
        }
    }

    // Filtreleri uygula
    foreach (var filter in filters)
    {
        if (filter.Value.Count > 0)
        {
            switch (filter.Key)
            {
                case "Brand":
                    query = query.Where(p => filter.Value.Contains(p.Brand.Id));
                    break;
                case "Category":
                    var allCategoryIds = await GetAllSubcategoryIds(filter.Value);
                    query = query.Where(p => allCategoryIds.Contains(p.CategoryId));
                    break;
                case "Price":
                    if (!string.IsNullOrWhiteSpace(filter.Value[0]))
                    {
                        var priceRange = filter.Value[0].Split('-');
                        if (priceRange.Length == 2)
                        {
                            if (decimal.TryParse(priceRange[0], out decimal minPrice))
                                query = query.Where(p => p.Price >= minPrice);
                            
                            if (decimal.TryParse(priceRange[1], out decimal maxPrice))
                                query = query.Where(p => p.Price <= maxPrice);
                        }
                    }
                    break;
                default:
                    // Özellik filtreleri
                    query = query.Where(p => p.ProductFeatureValues.Any(pfv =>
                        pfv.FeatureValue.Feature.Name == filter.Key &&
                        filter.Value.Contains(pfv.FeatureValue.Id)));
                    break;
            }
        }
    }

    // Sıralama
    query = sortOrder switch
    {
        "price_asc" => query.OrderBy(p => p.Price),
        "price_desc" => query.OrderByDescending(p => p.Price),
        _ => query.OrderByDescending(p => p.CreatedDate)
    };

    // Sayfalama
    return await query.ToPaginateAsync(pageRequest.PageIndex, pageRequest.PageSize);
}

    public async Task<List<FilterGroup>> GetAvailableFilters(string searchTerm)
{
    var filterDefinitions = new List<FilterGroup>();

    // Önce arama terimini kullanarak ürünleri filtreleyen bir sorgu oluştur
    IQueryable<Product> productsQuery = Context.Products
        .Include(p => p.Brand)
        .Include(p => p.Category)
        .Include(p => p.ProductFeatureValues)
        .ThenInclude(pfv => pfv.FeatureValue)
        .ThenInclude(fv => fv.Feature)
        .AsSplitQuery() 
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        var category = await Context.Categories.FirstOrDefaultAsync(c => c.Id == searchTerm);
        var brand = await Context.Brands.FirstOrDefaultAsync(b => b.Id == searchTerm);
        
        var terms = searchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.ToLower().Trim())
            .ToList();
        if (category != null)
        {
            var allSubcategoryIds = await GetAllSubcategoryIds(new List<string> { category.Id });
            productsQuery = productsQuery.Where(p => allSubcategoryIds.Contains(p.CategoryId));
        }
        else if (brand != null)
        {
            productsQuery = productsQuery.Where(p => p.BrandId == brand.Id);
        }
        else
        {
            
            foreach (var term in terms)
            {
                productsQuery = productsQuery.Where(p =>
                    EF.Functions.Like(p.Name.ToLower(), $"%{term}%") ||
                    EF.Functions.Like(p.Description.ToLower(), $"%{term}%") ||
                    EF.Functions.Like(p.Title.ToLower(), $"%{term}%") ||
                    EF.Functions.Like(p.Brand.Name.ToLower(), $"%{term}%") ||
                    EF.Functions.Like(p.Category.Name.ToLower(), $"%{term}%") ||
                    p.ProductFeatureValues.Any(pfv =>
                        EF.Functions.Like(pfv.FeatureValue.Name.ToLower(), $"%{term}%"))
                );
            }
        }
    }

    var products = await productsQuery.ToListAsync();

    // Category filter
    var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();
    var allRelevantCategories = await GetAllRelevantCategories(categoryIds);
    var categoryTree = BuildCategoryTree(allRelevantCategories);

    if (categoryTree.Any())
    {
        filterDefinitions.Add(new FilterGroup
        {
            Name = "Category",
            DisplayName = "Kategori",
            Type = FilterType.Checkbox,
            Options = GetCategoryOptions(categoryTree)
        });
    }

    // Brand filter
    var brands = products.Select(p => p.Brand)
        .Where(b => b != null)
        .DistinctBy(b => b.Id)
        .ToList();

    if (brands.Any())
    {
        filterDefinitions.Add(new FilterGroup
        {
            Name = "Brand",
            DisplayName = "Marka",
            Type = FilterType.Checkbox,
            Options = brands.Select(b => new FilterOption
            {
                Value = b.Id,
                DisplayValue = b.Name
            }).ToList()
        });
    }

    // Features filter
    var features = products
        .SelectMany(p => p.ProductFeatureValues)
        .Where(pfv => pfv.FeatureValue != null && pfv.FeatureValue.Feature != null)
        .GroupBy(pfv => pfv.FeatureValue.Feature.Name)
        .ToDictionary(
            g => g.Key,
            g => g.Select(pfv => pfv.FeatureValue).DistinctBy(fv => fv.Id).ToList()
        );

    foreach (var feature in features)
    {
        filterDefinitions.Add(new FilterGroup
        {
            Name = feature.Key,
            DisplayName = feature.Key,
            Type = FilterType.Checkbox,
            Options = feature.Value.Select(fv => new FilterOption
            {
                Value = fv.Id,
                DisplayValue = fv.Name
            }).ToList()
        });
    }

    // Price filter
    var prices = products.Select(p => p.Price).Where(price => price.HasValue).ToList();
    if (prices.Any())
    {
        var minPrice = prices.Min();
        var maxPrice = prices.Max();

        if (minPrice.HasValue && maxPrice.HasValue)
        {
            filterDefinitions.Add(new FilterGroup
            {
                Name = "Price",
                DisplayName = "Fiyat",
                Type = FilterType.Range,
                Options = GeneratePriceRanges(minPrice.Value, maxPrice.Value, 7)
                    .Select(r => new FilterOption
                    {
                        Value = $"{r.start}-{r.end}",
                        DisplayValue = $"{r.start:C0} - {r.end:C0}"
                    }).ToList()
            });
        }
    }

    return filterDefinitions;
}

    private List<(decimal start, decimal end)> GeneratePriceRanges(decimal minPrice, decimal maxPrice, int steps)
    {
        var ranges = new List<(decimal start, decimal end)>();
        if (minPrice >= maxPrice)
        {
            ranges.Add((minPrice, maxPrice));
            return ranges;
        }

        var step = (maxPrice - minPrice) / steps;
        for (int i = 0; i < steps; i++)
        {
            var start = minPrice + (step * i);
            var end = (i == steps - 1) ? maxPrice : minPrice + (step * (i + 1));
            ranges.Add((Math.Floor(start), Math.Ceiling(end)));
        }

        return ranges;
    }
    
    private async Task<List<string>> GetAllSubcategoryIds(List<string> categoryIds)
    {
        var allSubcategories = new HashSet<string>(categoryIds);
        var queue = new Queue<string>(categoryIds);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var subcategories = await Context.Categories
                .Where(c => c.ParentCategoryId == currentId)
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var subcategoryId in subcategories)
            {
                if (allSubcategories.Add(subcategoryId))
                {
                    queue.Enqueue(subcategoryId);
                }
            }
        }

        return allSubcategories.ToList();
    }

    private async Task<List<Category>> GetAllRelevantCategories(List<string> categoryIds)
    {
        var relevantCategoryIds = new HashSet<string>(categoryIds);
        var categoriesToProcess = new Queue<string>(categoryIds);

        while (categoriesToProcess.Count > 0)
        {
            var currentId = categoriesToProcess.Dequeue();
            var category = await Context.Categories
                .FirstOrDefaultAsync(c => c.Id == currentId);

            if (category != null)
            {
                if (category.ParentCategoryId != null && relevantCategoryIds.Add(category.ParentCategoryId))
                {
                    categoriesToProcess.Enqueue(category.ParentCategoryId);
                }

                var childCategories = await Context.Categories
                    .Where(c => c.ParentCategoryId == currentId)
                    .Select(c => c.Id)
                    .ToListAsync();

                foreach (var childId in childCategories)
                {
                    if (relevantCategoryIds.Add(childId))
                    {
                        categoriesToProcess.Enqueue(childId);
                    }
                }
            }
        }

        return await Context.Categories
            .Where(c => relevantCategoryIds.Contains(c.Id))
            .ToListAsync();
    }

    private List<Category> BuildCategoryTree(List<Category> allCategories)
    {
        var lookup = allCategories.ToLookup(c => c.ParentCategoryId);
    
        void AddSubCategories(Category category)
        {
            category.SubCategories = lookup[category.Id].ToList();
            foreach (var subCategory in category.SubCategories)
            {
                AddSubCategories(subCategory);
            }
        }

        var rootCategories = lookup[null].ToList();
        foreach (var rootCategory in rootCategories)
        {
            AddSubCategories(rootCategory);
        }

        return rootCategories;
    }

    private List<FilterOption> GetCategoryOptions(ICollection<Category> categories, string parentPath = "")
    {
        var options = new List<FilterOption>();

        foreach (var category in categories)
        {
            var currentPath = string.IsNullOrEmpty(parentPath) ? category.Name : $"{parentPath} > {category.Name}";
        
            options.Add(new FilterOption
            {
                Value = category.Id,
                DisplayValue = currentPath,
                ParentId = category.ParentCategoryId
            });

            if (category.SubCategories != null && category.SubCategories.Any())
            {
                options.AddRange(GetCategoryOptions(category.SubCategories, currentPath));
            }
        }

        return options;
    }
}