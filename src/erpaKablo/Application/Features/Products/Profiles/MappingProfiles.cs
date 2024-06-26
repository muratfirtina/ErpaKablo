using Application.Features.Products.Commands.Create;
using Application.Features.Products.Commands.Delete;
using Application.Features.Products.Commands.Update;
using Application.Features.Products.Dtos;
using Application.Features.Products.Queries.GetById;
using Application.Features.Products.Queries.GetList;
using AutoMapper;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;

namespace Application.Features.Products.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Product, CreateProductCommand>()
            .ForMember(dest=>dest.CreateProductDto,opt=>opt.MapFrom(src=>src))
            .ReverseMap();
        CreateMap<Product, CreatedProductResponse>()
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.BrandId))
            //.ForMember(dest => dest.ProductFeatures, opt => opt.MapFrom(src => src.Features))
            .ReverseMap();
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
            .ReverseMap();


        CreateMap<Product, GetByIdProductResponse>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
            //.ForMember(dest => dest.ProductFeatures, opt => opt.MapFrom(src => src.Features))
            .ReverseMap();

        CreateMap<Product, UpdateProductCommand>()
            .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.Brand.Id))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Category.Id))
            //.ForMember(dest => dest.ProductFeatures, opt => opt.MapFrom(src => src.Features))
            .ReverseMap()
            .ForMember(dest => dest.Brand, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore());

        CreateMap<Product, UpdatedProductResponse>()
          .ReverseMap();

        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.ProductImageFiles, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Brand, opt => opt.Ignore())
            .ReverseMap();
        
        CreateMap<Product, DeletedProductResponse>();





    }
}
