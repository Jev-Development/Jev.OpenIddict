using Jev.OpenIddict.Web.Constants;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using System.Collections.Immutable;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Jev.OpenIddict.Web.Extensions
{
    public static class SeedExtensions
    {
        public static async Task<IServiceProvider> SeedAdminRoleAsync(this IServiceProvider provider)
        {
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            if (await roleManager.FindByNameAsync(RoleConstants.Administrator) != null)
                return provider;


            await roleManager.CreateAsync(
                new IdentityRole()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = RoleConstants.Administrator
                });


            return provider;
        }

        public static async Task<IServiceProvider> SeedAdminApplicationAsync(this IServiceProvider provider)
        {
            var applicationManager = provider.GetRequiredService<OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication>>();

            var appDescriptors = new List<OpenIddictApplicationDescriptor>();

            if (await applicationManager.FindByClientIdAsync(ApplicationConstants.OpenIddictManagementApplication) == null)
            {
                appDescriptors.Add(new OpenIddictApplicationDescriptor()
                {
                    ClientId = ApplicationConstants.OpenIddictManagementApplication,
                    DisplayName = ApplicationConstants.OpenIddictManagementApplication,
                    PostLogoutRedirectUris = { new Uri("https://localhost:7083"), new Uri("http://localhost:5037"), new Uri("https://localhost:7191/authentication/logout-callback") },
                    RedirectUris = { new Uri("https://localhost:7191/authentication/login-callback") },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles
                    },
                    Type = ClientTypes.Public,
                    ConsentType = ConsentTypes.Implicit,
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }

            if (await applicationManager.FindByClientIdAsync(ApplicationConstants.TraderApplication) == null)
            {
                appDescriptors.Add(new OpenIddictApplicationDescriptor()
                {
                    ClientId = ApplicationConstants.TraderApplication,
                    DisplayName = ApplicationConstants.TraderApplication,
                    PostLogoutRedirectUris = { new Uri("https://localhost:7083"), new Uri("http://localhost:5037"), new Uri("https://localhost:7191/authentication/logout-callback") },
                    RedirectUris = { new Uri("https://localhost:7191/authentication/login-callback") },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles
                    },
                    ConsentType = ConsentTypes.Implicit,
                    Type = ClientTypes.Public,
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }

            foreach (var app in appDescriptors)
            {
                await applicationManager.CreateAsync(app);
            }

            return provider;
        }

        public static async Task<IServiceProvider> SeedAdminUserAsync(this IServiceProvider provider)
        {
            string adminEmail = "terrancejevon@gmail.com";
            var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();

            if (await userManager.FindByEmailAsync(adminEmail) != null)
                return provider;

            var user = new IdentityUser()
            {
                Id = Guid.NewGuid().ToString(),
                Email = adminEmail,
                UserName = adminEmail
            };

            await userManager.CreateAsync(user, adminEmail);

            await userManager.AddToRoleAsync(user, RoleConstants.Administrator);

            return provider;
        }
    }
}
