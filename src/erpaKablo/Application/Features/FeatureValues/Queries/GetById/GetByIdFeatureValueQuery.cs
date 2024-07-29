using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.FeatureValues.Queries.GetById;

public class GetByIdFeatureValueQuery : IRequest<GetByIdFeatureValueResponse>
{
    public string Id { get; set; }
    
    public class GetByIdFeatureValueQueryHandler : IRequestHandler<GetByIdFeatureValueQuery, GetByIdFeatureValueResponse>
    {
        private readonly IFeatureValueRepository _featureValueRepository;
        private readonly IMapper _mapper;

        public GetByIdFeatureValueQueryHandler(IFeatureValueRepository featureValueRepository, IMapper mapper)
        {
            _featureValueRepository = featureValueRepository;
            _mapper = mapper;
        }

        public async Task<GetByIdFeatureValueResponse> Handle(GetByIdFeatureValueQuery request, CancellationToken cancellationToken)
        {
            FeatureValue? featureValue = await _featureValueRepository.GetAsync(
                predicate: p => p.Id == request.Id,
                include: f => f.Include(f => f.Feature),
                cancellationToken: cancellationToken);
            GetByIdFeatureValueResponse response = _mapper.Map<GetByIdFeatureValueResponse>(featureValue);
            return response;
        }
    }
}