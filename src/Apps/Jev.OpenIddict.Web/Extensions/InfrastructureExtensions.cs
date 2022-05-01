
using Jev.OpenIddict.Core.Mappers;
using Jev.OpenIddict.Core.OpenIddict.Applications;
using Jev.OpenIddict.Domain.Configuration;
using Jev.OpenIddict.Entities;
using Jev.OpenIddict.Web.Controllers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Jev.OpenIddict.Web.Extensions
{
    public static class InfrastructureExtensions
    {

        public static IServiceCollection ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = false;
                options.Password.RequiredUniqueChars = 0;

            })
                .AddEntityFrameworkStores<IdServerContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
                options.ClaimsIdentity.EmailClaimType = Claims.Email;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login";
            });

            return services;
        }

        public static IServiceCollection ConfigureOpenIdConnect(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<IdServerContext>(options =>
            {
                options.UseNpgsql(connectionString, options =>
                {
                    options.MigrationsAssembly("Jev.OpenIddict.Migrations");
                });
            });

            services
                .AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                        .UseDbContext<IdServerContext>();
                })
                .AddServer(options =>
                {
                    
                    options.SetUserinfoEndpointUris("/connect/userinfo");
                    options.SetTokenEndpointUris(AuthorizationController.ConnectTokenPath);
                    options.SetAuthorizationEndpointUris(AuthorizationController.ConnectAuthorizePath);
                    options.SetLogoutEndpointUris(AuthorizationController.ConnectLogoutPath);

                    //options.AllowClientCredentialsFlow();
                    options.AllowAuthorizationCodeFlow();
                    options.AllowRefreshTokenFlow();

                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();

                    options.UseAspNetCore()
                        .EnableTokenEndpointPassthrough()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough()
                        .EnableStatusCodePagesIntegration();
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });


            return services;
        }

        public static IServiceCollection RegisterAutoMapper(this IServiceCollection services)
        {
            return services.AddAutoMapper(typeof(ApplicationProfile).Assembly);
        }

        public static IServiceCollection RegisterMediatR(this IServiceCollection services)
        {
            return services.AddMediatR(typeof(ApplicationHandler).Assembly);
        }

        public static IServiceCollection RegisterOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<AppOptions>(nameof(AppOptions));

            return services;
        }
    }
}
