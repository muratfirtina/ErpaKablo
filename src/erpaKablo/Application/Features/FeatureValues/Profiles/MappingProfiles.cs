using Application.Features.FeatureValues.Commands.Create;
using Application.Features.FeatureValues.Commands.Delete;
using Application.Features.FeatureValues.Commands.Update;
using Application.Features.FeatureValues.Queries.GetById;
using Application.Features.FeatureValues.Queries.GetList;
using Application.Features.Products.Dtos;
using AutoMapper;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;

namespace Application.Features.FeatureValues.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<FeatureValue, GetAllFeatureValueQueryResponse>().ReverseMap();
        CreateMap<FeatureValue, GetByIdFeatureValueResponse>().ReverseMap();
        CreateMap<List<FeatureValue>, GetListResponse<GetAllFeatureValueQueryResponse>>()
            .ForMember(dest 
                => dest.Items, opt 
                => opt.MapFrom(src => src));
        CreateMap<IPaginate<FeatureValue>, GetListResponse<GetAllFeatureValueQueryResponse>>().ReverseMap();
        
        CreateMap<FeatureValue, CreateFeatureValueCommand>().ReverseMap();
        CreateMap<FeatureValue, CreatedFeatureValueResponse>().ReverseMap();
        CreateMap<FeatureValue, UpdateFeatureValueCommand>().ReverseMap();
        CreateMap<FeatureValue, UpdatedFeatureValueResponse>().ReverseMap();
        CreateMap<FeatureValue, DeletedFeatureValueResponse>().ReverseMap();
        CreateMap<FeatureValue, FeatureValueDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
            .ReverseMap();
    }
}