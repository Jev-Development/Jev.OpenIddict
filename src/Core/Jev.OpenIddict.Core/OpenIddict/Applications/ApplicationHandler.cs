using AutoMapper;
using Jev.OpenIddict.Core.OpenIddict.Applications.Queries;
using Jev.OpenIddict.Domain.Dto.Application;
using MediatR;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

namespace Jev.OpenIddict.Core.OpenIddict.Applications
{
    public class ApplicationHandler : IRequestHandler<GetApplicationByClientIdQuery, ApplicationViewDto>
    {
        private readonly OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> _applicationManager;
        private readonly IMapper _mapper;

        public ApplicationHandler(
            OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> applicationManager,
            IMapper mapper)
        {
            _applicationManager = applicationManager ?? throw new ArgumentNullException(nameof(applicationManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApplicationViewDto> Handle(GetApplicationByClientIdQuery request, CancellationToken cancellationToken)
        {
            return _mapper.Map<ApplicationViewDto>(await _applicationManager.FindByClientIdAsync(request.ClientId) 
                ?? throw new InvalidOperationException($"Could not find the client with id: {request.ClientId}"));


        }
    }
}
