using Application.Features.Products.Commands.Create;
using Application.Features.Products.Dtos;
using Application.Features.Products.Queries;
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
                => opt.MapFrom(src => src.ProductFeatures.Select(pf => new ProductFeatureDto {
                FeatureGroupName = pf.FeatureGroupName,
                FeatureDetails = pf.Features.Select(f => $"{f.Name}: {f.Value}").ToList()
            }))).ReverseMap();


    }
}