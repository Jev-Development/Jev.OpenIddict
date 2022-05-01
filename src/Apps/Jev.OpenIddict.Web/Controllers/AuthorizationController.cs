using Jev.OpenIddict.Domain.Configuration;
using Jev.OpenIddict.Infrastructure.Attributes;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Server.AspNetCore;
using System.Net.Mime;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Jev.OpenIddict.Web.Controllers
{
    public class AuthorizationController : ControllerBase
    {
        public const string ConnectAuthorizePath = "/connect/authorize";
        public const string ConnectLogoutPath = "/connect/logout";
        public const string ConnectTokenPath = "/connect/token";
        public const string ConsentPath = "/consent";

        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> _applicationManager;
        private readonly OpenIddictAuthorizationManager<OpenIddictEntityFrameworkCoreAuthorization> _authorizationManager;
        private readonly OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope> _scopeManager;
        private readonly AppOptions _appOptions;

        public AuthorizationController(
            UserManager<IdentityUser> userManager,
            OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication> applicationManager,
            OpenIddictAuthorizationManager<OpenIddictEntityFrameworkCoreAuthorization> authorizationManager,
            SignInManager<IdentityUser> signInManager,
            OpenIddictScopeManager<OpenIddictEntityFrameworkCoreScope> scopeManager,
            IOptions<AppOptions> appOptions)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _applicationManager = applicationManager ?? throw new ArgumentNullException(nameof(applicationManager));
            _authorizationManager = authorizationManager ?? throw new ArgumentNullException(nameof(authorizationManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
            _appOptions = appOptions.Value ?? throw new ArgumentNullException(nameof(appOptions));
        }

        [IgnoreAntiforgeryToken]
        [HttpGet(ConnectAuthorizePath)]
        [HttpPost(ConnectAuthorizePath)]
        public async Task<IActionResult> AuthorizeAsync()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("Unable to retrieve the OpenId request.");

            if (request.HasPrompt(Prompts.Login))
            {
                var prompts = string.Join(" ", request.GetPrompts().Remove(Prompts.Login));

                var parameters = Request.Query.Where(param => param.Key != Parameters.Prompt).ToList();

                return Challenge(new AuthenticationProperties()
                {
                    RedirectUri = $"{Request.PathBase}{Request.Path}{QueryString.Create(parameters)}",
                }, IdentityConstants.ApplicationScheme);
            }

            var loginResult = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

            if (IsUnauthenticated(loginResult))
            {
                if (request.HasPrompt(Prompts.None))
                {
                    return Forbid(new AuthenticationProperties(new Dictionary<string, string?>()
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in"
                    }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                return Challenge(new AuthenticationProperties()
                {
                    RedirectUri = $"{Request.PathBase}{Request.Path}{QueryString.Create(Request.Query.ToList())}"
                }, IdentityConstants.ApplicationScheme);
            }

            var user = await _userManager.GetUserAsync(loginResult.Principal) ?? throw new InvalidOperationException("Could not retrieve user");

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId ?? string.Empty) ?? throw new InvalidOperationException("Could not retrieve application");

            var authorizations = await ToListAsync(
                _authorizationManager.FindAsync(user.Id,
                    application.Id ?? throw new ArgumentNullException(nameof(application.Id)),
                    Statuses.Valid,
                    AuthorizationTypes.Permanent,
                    request.GetScopes()));

            switch (application.ConsentType)
            {
                case ConsentTypes.External when !authorizations.Any():
                    return Forbid(new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The logged in user is not allowed to access the requested application."
                    }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                case ConsentTypes.Implicit:
                case ConsentTypes.External when authorizations.Any():
                case ConsentTypes.Explicit when authorizations.Any() && !request.HasPrompt(Prompts.Consent):
                    var principal = await _signInManager.CreateUserPrincipalAsync(user);

                    principal.SetScopes(request.GetScopes());
                    principal.SetResources(await ToListAsync(_scopeManager.ListResourcesAsync(principal.GetScopes())));

                    var authorization = authorizations.LastOrDefault();

                    if (authorization == null)
                    {
                        authorization = await _authorizationManager.CreateAsync(principal, user.Id, application.Id, AuthorizationTypes.Permanent, principal.GetScopes());
                    }

                    principal.SetAuthorizationId(authorization.Id);

                    foreach (var claim in principal.Claims)
                    {
                        claim.SetDestinations(GetDestinations(claim, principal));
                    }

                    return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                case ConsentTypes.Explicit when request.HasPrompt(Prompts.None):
                case ConsentTypes.Systematic when request.HasPrompt(Prompts.None):
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "Interactive user consent is required."
                        }));
                default:
                    return Redirect($"{Request.PathBase}{ConsentPath}{Request.QueryString}");
            }


            bool IsUnauthenticated(AuthenticateResult result)
            {
                return result == null || !result.Succeeded || (request?.MaxAge != null && result.Properties?.IssuedUtc != null && DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value));
            }


        }

        [Authorize]
        [HttpPost(ConnectAuthorizePath)]
        [ConsentFormSelector("accept", "deny")]
        public async Task<IActionResult> AcceptAsync([FromForm] bool? accept, [FromForm] bool? deny)
        {
            if (accept == null && deny == null)
                throw new ArgumentException("Unknown consent request.");

            var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            bool isAccepted = accept ?? !deny ?? false;

            if (!isAccepted)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            else
            {
                // Retrieve the profile of the logged in user.
                var user = await _userManager.GetUserAsync(User) ??
                    throw new InvalidOperationException("The user details cannot be retrieved.");

                // Retrieve the application details from the database.
                var application = await _applicationManager.FindByClientIdAsync(request.ClientId ?? throw new ArgumentNullException(nameof(request.ClientId))) ??
                    throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

                // Retrieve the permanent authorizations associated with the user and the calling client application.
                var authorizations = await ToListAsync(_authorizationManager.FindAsync(
                    subject: await _userManager.GetUserIdAsync(user),
                    client: application?.Id ?? throw new ArgumentNullException(nameof(application)),
                    status: Statuses.Valid,
                    type: AuthorizationTypes.Permanent,
                    scopes: request.GetScopes()));

                // Note: the same check is already made in the other action but is repeated
                // here to ensure a malicious user can't abuse this POST-only endpoint and
                // force it to return a valid response without the external authorization.
                if (!authorizations.Any() && await _applicationManager.HasConsentTypeAsync(application, ConsentTypes.External))
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "The logged in user is not allowed to access this client application."
                        }));
                }

                var principal = await _signInManager.CreateUserPrincipalAsync(user);

                // Note: in this sample, the granted scopes match the requested scope
                // but you may want to allow the user to uncheck specific scopes.
                // For that, simply restrict the list of scopes before calling SetScopes.
                principal.SetScopes(request.GetScopes());
                principal.SetResources(await ToListAsync(_scopeManager.ListResourcesAsync(principal.GetScopes())));


                // Automatically create a permanent authorization to avoid requiring explicit consent
                // for future authorization or token requests containing the same scopes.
                var authorization = authorizations.LastOrDefault();
                if (authorization == null)
                {
                    authorization = await _authorizationManager.CreateAsync(
                        principal: principal,
                        subject: user.Id,
                        client: application.Id ?? throw new ArgumentNullException("client"),
                        type: AuthorizationTypes.Permanent,
                        scopes: principal.GetScopes());
                }

                principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

                foreach (var claim in principal.Claims)
                {
                    claim.SetDestinations(GetDestinations(claim, principal));
                }

                // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

        }

        [HttpGet(ConnectLogoutPath)]
        [HttpPost(ConnectLogoutPath)]
        public async Task<IActionResult> SignOutAsync()
        {
            await _signInManager.SignOutAsync();
            return SignOut(new AuthenticationProperties()
            { 
                RedirectUri = _appOptions.BasePath
            }, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpPost(ConnectTokenPath), Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> ExchangeAsync()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException($"Could not retrieve the openId connect request.");

            if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
            {
                var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

                var user = await _userManager.GetUserAsync(principal);

                if (user == null)
                {
                    return Forbid(new AuthenticationProperties(new Dictionary<string, string?>()
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                    }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                foreach (var claim in principal?.Claims ?? throw new InvalidOperationException("Unexpected null values."))
                {
                    claim.SetDestinations(GetDestinations(claim, principal));
                }

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new InvalidOperationException("The specified grant type is not supported.");
        }


        private IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
        {
            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            switch (claim.Type)
            {
                case Claims.Name:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Email))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

                    if (principal.HasScope(Scopes.Roles))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Subject:
                    yield return Destinations.AccessToken;

                    yield return Destinations.IdentityToken;

                    yield break;

                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                case "AspNet.Identity.SecurityStamp": yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }

        private async Task<IEnumerable<T>> ToListAsync<T>(IAsyncEnumerable<T> list)
        {
            var result = new List<T>();

            await foreach (var item in list)
                result.Add(item);

            return result;
        }


    }
}
