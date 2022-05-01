using Jev.OpenIddict.Domain.Configuration;
using Jev.OpenIddict.Domain.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security;

namespace Jev.OpenIddict.Web.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        public const string LogoutPath = "logout";

        private const string SignInPath = "login";
        private const string RegisterPath = "register";
        private const string LoggedInPath = "logged-in";
        private const string ReturnUrlKey = "ReturnUrl";

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly AppOptions _appOptions;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IOptions<AppOptions> options)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _appOptions = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        [AllowAnonymous]
        [HttpPost(SignInPath)]
        public async Task<IActionResult> SignInAsync([FromForm] string username, [FromForm] string password, [FromForm] string? rememberMe)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return Unauthorized();

            var signInResult = await _signInManager.PasswordSignInAsync(user, password, rememberMe == "on", false);

            await _signInManager.SignInAsync(user, rememberMe == "on");

            if (signInResult.Succeeded)
                return Request.Query.Any(query => query.Key == ReturnUrlKey) ? Redirect(Request.Query[ReturnUrlKey]) : Redirect($"{_appOptions.BasePath}");
            else
                return Unauthorized();
        }

        [AllowAnonymous]
        [HttpGet(LoggedInPath)]
        public IActionResult LoggedIn()
        {
            return Ok(User?.Identity?.IsAuthenticated);
        }

        [Authorize]
        [HttpGet(LogoutPath)]
        public async Task<IActionResult> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return Redirect($"{_appOptions.BasePath}/{SignInPath}");
        }

        [AllowAnonymous]
        [HttpPost(RegisterPath)]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto dto)
        {
            var newUser = new IdentityUser()
            {
                Id = Guid.NewGuid().ToString(),
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true
            };
            var userCreateResult = await _userManager.CreateAsync(newUser, dto.Password);

            if (!userCreateResult.Succeeded)
            {
                throw new ArgumentException(JoinErrors(userCreateResult.Errors.Select(err => err.Description)));
            }

            return Ok(newUser.Id);
        }

        private string JoinErrors(IEnumerable<string> errors) => string.Join(", ", errors);
    }
}
