using Jev.OpenIddict.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jev.OpenIddict.Web
{
    public class IdServerContextDesignTimeFactory : IDesignTimeDbContextFactory<IdServerContext>
    {
        public IdServerContext CreateDbContext(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .Build();

            var builder = new DbContextOptionsBuilder();

            builder.UseNpgsql(configuration["ConnectionString"], options =>
            {
                options.MigrationsAssembly("Jev.OpenIddict.Migrations");
            });

            return new IdServerContext(builder.Options);
        }
    }
}
