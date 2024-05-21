using Application.Features.Filters.Commands.Create;
using Application.Features.Filters.Commands.Delete;
using Application.Features.Filters.Commands.Update;
using Application.Features.Filters.Queries.GetById;
using Application.Features.Filters.Queries.GetList;
using AutoMapper;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;

namespace Application.Features.Filters.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Filter, GetAllFilterQueryResponse>().ReverseMap();
        CreateMap<Filter, GetByIdFilterResponse>().ReverseMap();
        CreateMap<List<Filter>, GetListResponse<GetAllFilterQueryResponse>>()
            .ForMember(dest 
                => dest.Items, opt 
                => opt.MapFrom(src => src));
        CreateMap<IPaginate<Filter>, GetListResponse<GetAllFilterQueryResponse>>().ReverseMap();
        
        CreateMap<Filter, CreateFilterCommand>().ReverseMap();
        CreateMap<Filter, CreatedFilterResponse>().ReverseMap();
        CreateMap<Filter, UpdateFilterCommand>().ReverseMap();
        CreateMap<Filter, UpdatedFilterResponse>().ReverseMap();
        CreateMap<Filter, DeletedFilterResponse>().ReverseMap();
    }
}