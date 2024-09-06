using Application.Features.Brands.Commands.Create;
using Application.Features.Brands.Commands.Delete;
using Application.Features.Brands.Commands.Update;
using Application.Features.Brands.Dtos;
using Application.Features.Brands.Queries.GetByDynamic;
using Application.Features.Brands.Queries.GetById;
using Application.Features.Brands.Queries.GetList;
using AutoMapper;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;

namespace Application.Features.Brands.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Brand, GetAllBrandQueryResponse>()
            .ForMember(dest 
                    => dest.BrandImage, opt
                    => opt.MapFrom(src =>
                    src.BrandImageFiles.FirstOrDefault()));
        CreateMap<Brand, GetByIdBrandResponse>()
            .ForMember(dest => dest.BrandImage, opt => opt.MapFrom(src => 
                src.BrandImageFiles != null && src.BrandImageFiles.Any() 
                    ? new BrandImageFileDto { Url = src.BrandImageFiles.First().Url } 
                    : null));
        CreateMap<List<Brand>, GetListResponse<GetAllBrandQueryResponse>>()
            .ForMember(dest 
                => dest.Items, opt 
                => opt.MapFrom(src => src));
        CreateMap<IPaginate<Brand>, GetListResponse<GetAllBrandQueryResponse>>()
            .ForMember(dest 
                => dest.Items, opt 
                => opt.MapFrom(src => src.Items))
            .ReverseMap();

        CreateMap<Brand, GetListBrandByDynamicDto>()
            .ForMember(dest 
                => dest.BrandImage, opt 
                => opt.MapFrom(src => src.BrandImageFiles.FirstOrDefault()));


        CreateMap<List<Brand>, GetListResponse<GetListBrandByDynamicDto>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src));
        
        CreateMap<IPaginate<Brand>, GetListResponse<GetListBrandByDynamicDto>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ReverseMap();

        CreateMap<IPaginate<Brand>, List<GetListBrandByDynamicDto>>();
        
        CreateMap<Brand, CreateBrandCommand>().ReverseMap();
        CreateMap<Brand, CreatedBrandResponse>()
            .ForMember(dest => dest.BrandImage, opt => opt.MapFrom(src => 
                src.BrandImageFiles != null && src.BrandImageFiles.Any() 
                    ? new BrandImageFileDto { Url = src.BrandImageFiles.First().Url } 
                    : null));
        
        CreateMap<Brand, UpdateBrandCommand>().ReverseMap();
        CreateMap<Brand, UpdatedBrandResponse>().ReverseMap();
        CreateMap<Brand, DeletedBrandResponse>().ReverseMap();
        CreateMap<BrandImageFile, BrandImageFileDto>()
            .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.Name))
            .ReverseMap();
    }
}