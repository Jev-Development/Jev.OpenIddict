using Jev.OpenIddict.Core.OpenIddict.Applications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jev.OpenIddict.Web.Controllers.V1
{
    [ApiController]
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ApplicationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ApplicationController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet]
        [Route("{clientId}")]
        public async Task<IActionResult> GetAsync([FromRoute] string clientId)
        {
            return Ok(await _mediator.Send(new GetApplicationByClientIdQuery(clientId)));
        }
    }
}
