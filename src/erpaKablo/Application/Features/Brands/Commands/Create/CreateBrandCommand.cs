using Application.Features.Brands.Rules;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Brands.Commands.Create;

public class CreateBrandCommand : IRequest<CreatedBrandResponse>
{
    public string Name { get; set; }
    public List<IFormFile>? BrandImage { get; set; }
    
    public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, CreatedBrandResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBrandRepository _brandRepository;
        private readonly IStorageService _storageService;
        private readonly BrandBusinessRules _brandBusinessRules;

        public CreateBrandCommandHandler(IMapper mapper, IBrandRepository brandRepository, IStorageService storageService, BrandBusinessRules brandBusinessRules)
        {
            _mapper = mapper;
            _brandRepository = brandRepository;
            _storageService = storageService;
            _brandBusinessRules = brandBusinessRules;
        }

        public async Task<CreatedBrandResponse> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
        {
            await _brandBusinessRules.BrandNameShouldNotExistWhenInsertingOrUpdating(request.Name);
            
            var brand = _mapper.Map<Brand>(request);
            await _brandRepository.AddAsync(brand);

            if (request.BrandImage != null)
            {
                var uploadResult = await _storageService.UploadAsync("brands", brand.Id, request.BrandImage);
                if (uploadResult.Any())
                {
                    var (fileName, path, _, storageType) = uploadResult.First();
                    var brandImageFile = new BrandImageFile(fileName, "brands", path, storageType);
                    brand.BrandImageFiles = new List<BrandImageFile> { brandImageFile };
                    await _brandRepository.UpdateAsync(brand);
                }
            }
            
            CreatedBrandResponse response = _mapper.Map<CreatedBrandResponse>(brand);
            return response;
        }
    }
}