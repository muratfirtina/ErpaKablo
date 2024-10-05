using Application.Features.Brands.Dtos;
using Application.Features.Carousels.Dtos;
using Application.Features.Categories.Dtos;
using Application.Features.Categories.Queries.GetById;
using Application.Features.ProductImageFiles.Dtos;
using Application.Features.Products.Dtos;
using Application.Storage;
using Core.Application.Responses;

namespace Application.Extensions
{
    public static class ProductImageExtensions
    {
        public static void SetImageUrls<T>(this IEnumerable<T> items, IStorageService storageService) where T : class
        {
            var baseUrl = storageService.GetStorageUrl();
            foreach (var item in items)
            {
                SetImageUrl(item, baseUrl);
            }
        }

        public static void SetImageUrl<T>(this T item, IStorageService storageService) where T : class
        {
            var baseUrl = storageService.GetStorageUrl();
            SetImageUrl(item, baseUrl);
        }

        private static void SetImageUrl<T>(T item, string baseUrl) where T : class
        {
            if (item is IHasShowcaseImage showcaseItem)
            {
                SetShowcaseImageUrl(showcaseItem, baseUrl);
            }

            if (item is IHasProductImageFiles productImagesItem)
            {
                foreach (var imageFile in productImagesItem.ProductImageFiles)
                {
                    SetProductImageFileUrl(imageFile, baseUrl);
                }
            }

            if (item is IHasRelatedProducts relatedProductsItem)
            {
                foreach (var relatedProduct in relatedProductsItem.RelatedProducts)
                {
                    SetShowcaseImageUrl(relatedProduct, baseUrl);
                }
            }

            if (item is IHasCategoryImage categoryItem && categoryItem.CategoryImage != null)
            {
                SetCategoryImageUrl(categoryItem.CategoryImage, baseUrl);
            }

            if (item is IHasBrandImage brandItem)
            {
                SetBrandImageUrl(brandItem.BrandImage, baseUrl);
            }

            if (item is IHasCarouselImageFiles carouselItem)
            {
                foreach (var imageFile in carouselItem.CarouselImageFiles)
                {
                    SetCarouselImageUrl(imageFile, baseUrl);
                }
            }
        }

        private static void SetShowcaseImageUrl(IHasShowcaseImage item, string baseUrl)
        {
            if (item.ShowcaseImage == null)
            {
                item.ShowcaseImage = new ProductImageFileDto
                {
                    EntityType = "products",
                    Path = "",
                    FileName = "ecommerce-default-product.png"
                };
            }

            SetProductImageFileUrl(item.ShowcaseImage, baseUrl);
        }

        private static void SetProductImageFileUrl(ProductImageFileDto imageFile, string baseUrl)
        {
            imageFile.Url = imageFile.FileName == "ecommerce-default-product.png"
                ? $"{baseUrl}{imageFile.EntityType}/{imageFile.FileName}"
                : $"{baseUrl}{imageFile.EntityType}/{imageFile.Path}/{imageFile.FileName}";
        }
        
        private static void SetCategoryImageUrl(CategoryImageFileDto? imageFile, string baseUrl)
        {
            if (imageFile == null) return;

            imageFile.Url = imageFile.FileName == "ecommerce-default-category.png"
                ? $"{baseUrl}{imageFile.EntityType}/{imageFile.FileName}"
                : $"{baseUrl}{imageFile.EntityType}/{imageFile.Path}/{imageFile.FileName}";
        }
        
        private static void SetBrandImageUrl(BrandImageFileDto imageFile, string baseUrl)
        {
            imageFile.Url = imageFile.FileName == "ecommerce-default-brand.png"
                ? $"{baseUrl}{imageFile.EntityType}/{imageFile.FileName}"
                : $"{baseUrl}{imageFile.EntityType}/{imageFile.Path}/{imageFile.FileName}";
        }
        
        private static void SetCarouselImageUrl(CarouselImageFileDto imageFile, string baseUrl)
        {
            imageFile.Url = imageFile.FileName == "ecommerce-default-carousel.png"
                ? $"{baseUrl}{imageFile.EntityType}/{imageFile.FileName}"
                : $"{baseUrl}{imageFile.EntityType}/{imageFile.Path}/{imageFile.FileName}";
        }
    }

    public interface IHasShowcaseImage
    {
        ProductImageFileDto ShowcaseImage { get; set; }
    }

    public interface IHasProductImageFiles
    {
        ICollection<ProductImageFileDto> ProductImageFiles { get; set; }
    }

    public interface IHasRelatedProducts
    {
        List<RelatedProductDto> RelatedProducts { get; set; }
    }
    
    public interface IHasCategoryImage
    {
        CategoryImageFileDto CategoryImage { get; set; }
    }
    public interface IHasBrandImage
    {
        BrandImageFileDto BrandImage { get; set; }
    }
    public interface IHasCarouselImageFiles
    {
        ICollection<CarouselImageFileDto> CarouselImageFiles { get; set; }
    }
    
}