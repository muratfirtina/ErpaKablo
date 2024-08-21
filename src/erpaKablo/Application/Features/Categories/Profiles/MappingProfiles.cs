using Application.Features.Categories.Commands.Create;
using Application.Features.Categories.Commands.Delete;
using Application.Features.Categories.Commands.Update;
using Application.Features.Categories.Dtos;
using Application.Features.Categories.Queries.GetByDynamic;
using Application.Features.Categories.Queries.GetById;
using Application.Features.Categories.Queries.GetList;
using Application.Features.Categories.Queries.GetMainCategories;
using Application.Features.Features.Commands.Create;
using Application.Features.Features.Dtos;
using Application.Features.FeatureValues.Dtos;
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
            .ForMember(dest
                =>dest.CategoryImage,opt
                =>opt.MapFrom(src
                    => src.CategoryImageFiles.FirstOrDefault()));
        
        CreateMap<Category, GetListSubCategoryDto>().ReverseMap();
        
        CreateMap<Category, GetByIdCategoryResponse>()
            .ForMember(dest=>dest.ParentCategoryName,opt=>opt.MapFrom(src=> src.ParentCategory.Name))
            .ForMember(dest=>dest.SubCategories,opt=>opt.MapFrom(src=> src.SubCategories))
            .ForMember(dest=>dest.Features,opt=>opt.MapFrom(src=> src.Features))
            .ForMember(dest=>dest.FeatureValueProductCounts,opt=>opt.Ignore())
            .ForMember(dest=>dest.CategoryImage,opt=>opt.MapFrom(src=> src.CategoryImageFiles.FirstOrDefault()))
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
            .ForMember(dest => dest.CategoryImage, opt => opt.MapFrom(src => src.CategoryImageFiles.FirstOrDefault()));
        
        CreateMap<Category, UpdateCategoryCommand>().ReverseMap();
        CreateMap<Category, UpdatedCategoryResponse>().ReverseMap();
        CreateMap<Category, DeletedCategoryResponse>().ReverseMap();
        
        CreateMap<Category, GetListCategoryByDynamicDto>().ReverseMap();
        CreateMap<Category, GetListResponse<GetListCategoryByDynamicDto>>().ReverseMap();
        
        CreateMap<Feature, FeatureDto>()
            .ForMember(dest => dest.FeatureValues, opt => opt.MapFrom(src => src.FeatureValues));
        CreateMap<FeatureValue, FeatureValueDto>();
        CreateMap<CreateCategoryFeatureDto, CategoryFeature>().ReverseMap();
        CreateMap<Category, CreateCategoryResponseDto>()
                  //  .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.CategoryFeatures.Select(cf => cf.Feature)))
            .ReverseMap();
        
        CreateMap<Category, CategoryDto>()
            .ReverseMap();

        CreateMap<List<Category>, GetListResponse<GetListCategoryByDynamicDto>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src));

        CreateMap<Category, GetListCategoryByDynamicDto>()
            .ForMember(dest => dest.SubCategories, 
                opt => opt.MapFrom(src => src.SubCategories))
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products));
        
        CreateMap<IPaginate<Category>, GetListResponse<GetListCategoryByDynamicDto>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ReverseMap();

        CreateMap<IPaginate<Category>, List<GetListCategoryByDynamicDto>>();
        
        CreateMap<CategoryImageFile, CategoryImageFileDto>()
            .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.Name))
            .ReverseMap();
        
        CreateMap<Category, GetListCategoryByDynamicDto>();
        CreateMap<CategoryImageFile, CategoryImageFileDto>
            ().ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.Name))
            .ReverseMap();
        
        CreateMap<Category, GetMainCategoriesResponse>()
            .ForMember(dest => dest.CategoryImage, opt => opt.MapFrom(src => src.CategoryImageFiles.FirstOrDefault()));
        
        CreateMap<List<Category>, GetListResponse<GetMainCategoriesResponse>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src));
        
        CreateMap<IPaginate<Category>, GetListResponse<GetMainCategoriesResponse>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ReverseMap();
    }
    
}
