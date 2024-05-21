using Application.Features.Categories.Commands.Create;
using Application.Features.Categories.Commands.Delete;
using Application.Features.Categories.Commands.Update;
using Application.Features.Categories.Dtos;
using Application.Features.Categories.Queries.GetById;
using Application.Features.Categories.Queries.GetList;
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
        
        CreateMap<Category, CreateCategoryCommand>().ReverseMap();
        CreateMap<Category, CreatedCategoryResponse>().ReverseMap();
        CreateMap<Category, UpdateCategoryCommand>().ReverseMap();
        CreateMap<Category, UpdatedCategoryResponse>().ReverseMap();
        CreateMap<Category, DeletedCategoryResponse>().ReverseMap();
    }
}