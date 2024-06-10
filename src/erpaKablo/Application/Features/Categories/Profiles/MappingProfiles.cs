using Application.Features.Categories.Commands.Create;
using Application.Features.Categories.Commands.Delete;
using Application.Features.Categories.Commands.Update;
using Application.Features.Categories.Dtos;
using Application.Features.Categories.Queries.GetByDynamic;
using Application.Features.Categories.Queries.GetById;
using Application.Features.Categories.Queries.GetList;
using Application.Features.Features.Commands.Create;
using Application.Features.Features.Dtos;
using Application.Features.Products.Dtos;
using AutoMapper;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;

namespace Application.Features.Categories.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Category, GetAllCategoryQueryResponse>()
            .ForMember(dest=>dest.SubCategories,opt=>opt.MapFrom(src=> src.SubCategories))
            .ReverseMap();
        CreateMap<Category, GetListSubCategoryDto>().ReverseMap();
        CreateMap<Category, GetByIdCategoryResponse>()
            .ForMember(dest=>dest.ParentCategoryName,opt=>opt.MapFrom(src=> src.ParentCategory.Name))
            .ReverseMap();
        CreateMap<List<Category>, GetListResponse<GetAllCategoryQueryResponse>>()
            .ForMember(dest 
                => dest.Items, opt 
                => opt.MapFrom(src => src));
        CreateMap<IPaginate<Category>, GetListResponse<GetAllCategoryQueryResponse>>().ReverseMap();
        
        CreateMap<CreateCategoryCommand, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            //.ForMember(dest => dest.CategoryFeatures, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<Category, CreatedCategoryResponse>()
          //  .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.CategoryFeatures.Select(cf => cf.Feature)))
            .ReverseMap();
        CreateMap<Category, UpdateCategoryCommand>().ReverseMap();
        CreateMap<Category, UpdatedCategoryResponse>().ReverseMap();
        CreateMap<Category, DeletedCategoryResponse>().ReverseMap();
        
        CreateMap<Category, GetListCategoryByDynamicDto>().ReverseMap();
        CreateMap<IPaginate<Category>, GetListResponse<GetListCategoryByDynamicDto>>().ReverseMap();
        
        CreateMap<Feature, FeatureDto>()
            .ForMember(dest => dest.FeatureValues, opt => opt.MapFrom(src => src.FeatureValues));
        CreateMap<FeatureValue, FeatureValueDto>();
        CreateMap<CreateCategoryFeatureDto, CategoryFeature>().ReverseMap();
        CreateMap<Category, CreateCategoryResponseDto>()
                  //  .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.CategoryFeatures.Select(cf => cf.Feature)))
            .ReverseMap();
        
        CreateMap<Category, CategoryDto>()
            .ReverseMap();
        
        

    }
}
