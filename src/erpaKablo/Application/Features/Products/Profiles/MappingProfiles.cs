using Application.Features.Products.Commands.Create;
using Application.Features.Products.Commands.Update;
using Application.Features.Products.Dtos;
using Application.Features.Products.Queries.GetById;
using Application.Features.Products.Queries.GetList;
using AutoMapper;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;

namespace Application.Features.Products.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Product, CreateProductCommand>().ReverseMap();
        CreateMap<Product, CreatedProductResponse>().ReverseMap();
        CreateMap<List<Product>, GetListResponse<GetAllProductQueryResponse>>()
            .ForMember(dest 
                => dest.Items, opt 
                => opt.MapFrom(src => src));
        CreateMap<IPaginate<Product>, GetListResponse<GetAllProductQueryResponse>>()
            .ReverseMap();
        
        CreateMap<Product, GetAllProductQueryResponse>()
            .ForMember(dest 
                => dest.CategoryName, opt 
                => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest 
                => dest.BrandName, opt 
                => opt.MapFrom(src => src.Brand.Name))
            .ForMember(dest => dest.ProductFeatures, opt 
                => opt.MapFrom(src => src.ProductFeatures.Select(pf => 
                    new ProductFeatureDto
                    {
                        ProductFeatureGroupName = pf.Name,
                        FeatureDetails = pf.Features.Select(f => new FeatureDto
                        {
                            Name = f.Name,
                            Value = f.Value
                        }).ToList()
            }))).ReverseMap();

        CreateMap<Product, GetByIdProductResponse>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
            .ForMember(dest => dest.ProductFeatures, opt => opt.MapFrom(src => src.ProductFeatures.Select(pf =>
                new ProductFeatureDto
                {
                    ProductFeatureGroupName = pf.Name,
                    FeatureDetails = pf.Features.Select(f => new FeatureDto
                    {
                        Name = f.Name,
                        Value = f.Value
                    }).ToList()
                })))
            .ReverseMap();

        CreateMap<Product, UpdateProductCommand>()
            .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.Brand.Id))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Category.Id))
            /*.ForMember(dest => dest.ProductFeatures, opt => opt.MapFrom(src => src.ProductFeatures.Select(pf =>
                new ProductFeatureDto
                {
                    ProductFeatureGroupName = pf.Name,
                    FeatureDetails = pf.Features.Select(f => new FeatureDto
                    {
                        Name = f.Name,
                        Value = f.Value
                    }).ToList()
                })))*/
            .ReverseMap()
            .ForMember(dest => dest.Brand, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore());

        CreateMap<Product, UpdatedProductResponse>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
            .ForMember(dest => dest.ProductFeatures, opt => opt.MapFrom(src => src.ProductFeatures.Select(pf =>
                new ProductFeatureDto
                {
                    ProductFeatureGroupName = pf.Name,
                    FeatureDetails = pf.Features.Select(f => new FeatureDto
                    {
                        Name = f.Name,
                        Value = f.Value
                    }).ToList()
                })))
            .ReverseMap();
        
        CreateMap<ProductFeatureDto, ProductFeature>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProductFeatureGroupName))
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.FeatureDetails.Select(f => new Feature
            {
                Name = f.Name,
                Value = f.Value
            })))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ReverseMap();
    }
}
