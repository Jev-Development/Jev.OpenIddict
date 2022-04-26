using Jev.OpenIddict.Domain.Dto.Application;
using MediatR;

namespace Jev.OpenIddict.Core.OpenIddict.Applications.Queries
{
    public class GetApplicationByClientIdQuery : IRequest<ApplicationViewDto>
    {
        public string ClientId { get; set; }

        public GetApplicationByClientIdQuery(string clientId)
        { 
            ClientId = clientId;
        }
    }
}
