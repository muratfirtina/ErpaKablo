using Application.Features.Features.Commands.Create;
using Application.Features.Features.Commands.Delete;
using Application.Features.Features.Commands.Update;
using Application.Features.Features.Queries.GetById;
using Application.Features.Features.Queries.GetList;
using Application.Features.Products.Dtos;
using AutoMapper;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;

namespace Application.Features.Features.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Feature, GetAllFeatureQueryResponse>().ReverseMap();
        CreateMap<Feature, GetByIdFeatureResponse>().ReverseMap();
        CreateMap<List<Feature>, GetListResponse<GetAllFeatureQueryResponse>>()
            .ForMember(dest 
                => dest.Items, opt 
                => opt.MapFrom(src => src));
        CreateMap<IPaginate<Feature>, GetListResponse<GetAllFeatureQueryResponse>>().ReverseMap();
        
        CreateMap<Feature, CreateFeatureCommand>().ReverseMap();
        CreateMap<Feature, CreatedFeatureResponse>().ReverseMap();
        CreateMap<Feature, UpdateFeatureCommand>().ReverseMap();
        CreateMap<Feature, UpdatedFeatureResponse>().ReverseMap();
        CreateMap<Feature, DeletedFeatureResponse>().ReverseMap();
        CreateMap<Feature, ProductFeatureDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.FeatureValues, opt => opt.MapFrom(src => src.FeatureValues))
            .ReverseMap();

        CreateMap<Feature, ProductFeatureDto>()
            .ForMember(dest => dest.FeatureValues, opt => opt.MapFrom(src => src.FeatureValues.Select(fv =>
                new FeatureValueDto
                {
                    Id = fv.Id,
                    Value = fv.Value
                }).ToList()))
            .ReverseMap();
        
        
        CreateMap<FeatureValue, FeatureValueDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));
        
        CreateMap<ProductFeatureDto, FeatureValue>()
            .ForMember(dest => dest.Value,
                opt => opt.MapFrom(src => src.FeatureValues.Select(f => f.Value).FirstOrDefault()))
            .ReverseMap();
    }
}