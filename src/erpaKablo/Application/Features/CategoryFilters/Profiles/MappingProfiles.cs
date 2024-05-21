using Application.Features.CategoryFilters.Commands.Create;
using Application.Features.CategoryFilters.Commands.Delete;
using Application.Features.CategoryFilters.Commands.Update;
using Application.Features.CategoryFilters.Queries.GetById;
using Application.Features.CategoryFilters.Queries.GetList;
using AutoMapper;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;

namespace Application.Features.CategoryFilters.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<CategoryFilter, GetAllCategoryFilterQueryResponse>().ReverseMap();
        CreateMap<CategoryFilter, GetByIdCategoryFilterResponse>().ReverseMap();
        CreateMap<List<CategoryFilter>, GetListResponse<GetAllCategoryFilterQueryResponse>>()
            .ForMember(dest 
                => dest.Items, opt 
                => opt.MapFrom(src => src));
        CreateMap<IPaginate<CategoryFilter>, GetListResponse<GetAllCategoryFilterQueryResponse>>().ReverseMap();
        
        CreateMap<CategoryFilter, CreateCategoryFilterCommand>().ReverseMap();
        CreateMap<CategoryFilter, CreatedCategoryFilterResponse>().ReverseMap();
        CreateMap<CategoryFilter, UpdateCategoryFilterCommand>().ReverseMap();
        CreateMap<CategoryFilter, UpdatedCategoryFilterResponse>().ReverseMap();
        CreateMap<CategoryFilter, DeletedCategoryFilterResponse>().ReverseMap();
    }
}