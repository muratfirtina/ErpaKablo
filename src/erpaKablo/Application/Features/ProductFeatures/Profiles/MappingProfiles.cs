using Application.Features.ProductFeatures.Commands.Create;
using Application.Features.ProductFeatures.Commands.Delete;
using Application.Features.ProductFeatures.Commands.Update;
using Application.Features.ProductFeatures.Queries.GetById;
using Application.Features.ProductFeatures.Queries.GetList;
using AutoMapper;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;

namespace Application.Features.ProductFeatures.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<ProductFeature, GetAllProductFeatureQueryResponse>().ReverseMap();
        CreateMap<ProductFeature, GetByIdProductFeatureResponse>().ReverseMap();
        CreateMap<List<ProductFeature>, GetListResponse<GetAllProductFeatureQueryResponse>>()
            .ForMember(dest 
                => dest.Items, opt 
                => opt.MapFrom(src => src));
        CreateMap<IPaginate<ProductFeature>, GetListResponse<GetAllProductFeatureQueryResponse>>().ReverseMap();
        
        CreateMap<ProductFeature, CreateProductFeatureCommand>().ReverseMap();
        CreateMap<ProductFeature, CreatedProductFeatureResponse>().ReverseMap();
        CreateMap<ProductFeature, UpdateProductFeatureCommand>().ReverseMap();
        CreateMap<ProductFeature, UpdatedProductFeatureResponse>().ReverseMap();
        CreateMap<ProductFeature, DeletedProductFeatureResponse>().ReverseMap();
    }
}