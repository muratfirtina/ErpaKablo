using Application.Features.Brands.Rules;
using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Brands.Commands.Update;

public class UpdateBrandCommand : IRequest<UpdatedBrandResponse>
{
    public string Id { get; set; }
    public string Name { get; set; }

    public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand, UpdatedBrandResponse>
    {
        private readonly IBrandRepository _brandRepository;
        private readonly BrandBusinessRules _brandBusinessRules;
        private readonly IMapper _mapper;

        public UpdateBrandCommandHandler(IBrandRepository brandRepository, IMapper mapper, BrandBusinessRules brandBusinessRules)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
            _brandBusinessRules = brandBusinessRules;
        }

        public async Task<UpdatedBrandResponse> Handle(UpdateBrandCommand request,
            CancellationToken cancellationToken)
        {
            Brand? brand = await _brandRepository.GetAsync(p => p.Id == request.Id,
                cancellationToken: cancellationToken);
            await _brandBusinessRules.BrandShouldExistWhenSelected(brand);
            if (brand != null)
            {
                brand = _mapper.Map(request, brand);
                await _brandRepository.UpdateAsync(brand);
                UpdatedBrandResponse response = _mapper.Map<UpdatedBrandResponse>(brand);
                return response;
            }
            throw new Exception("Brand not found");
        }
    }
}