using Jev.OpenIddict.Infrastructure.Middleware;
using Jev.OpenIddict.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration["ConnectionString"];

builder.Services.AddControllers();
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("corski", policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowAnyOrigin();
    });
});

builder.Services.ConfigureOpenIdConnect(connectionString);
builder.Services.ConfigureIdentity();
builder.Services.RegisterAutoMapper();
builder.Services.RegisterMediatR();
builder.Services.AddApiVersioning(opts =>
{
    opts.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    opts.AssumeDefaultVersionWhenUnspecified = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.SeedAdminApplicationAsync();
    await scope.ServiceProvider.SeedAdminRoleAsync();
    await scope.ServiceProvider.SeedAdminUserAsync();
}

app.UseCors("corski");

app.UseErrorResponseMiddleware();
app.UseStaticFiles();
app.UseApiVersioning();

app.Use(async (context, next) =>
{
    string[] passthroughSegments = new string[] { "/api", "/connect", "/.well-known" };
    if (!passthroughSegments.Any(path => context.Request.Path.StartsWithSegments(path)))
    {
        if (string.IsNullOrEmpty(StartupMemory.IndexHtml))
        {
            using (var streamReader = new StreamReader(new FileStream(Path.Combine(app.Environment.WebRootPath, "index.html"), FileMode.Open, FileAccess.Read)))
            {
                StartupMemory.IndexHtml = await streamReader.ReadToEndAsync();
            }
        }

        context.Response.StatusCode = 200;
        await context.Response.WriteAsync(StartupMemory.IndexHtml);
    }
    else
    {
        await next();
    }
});


app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints((endpoints) =>
{
    endpoints.MapControllers();
});

app.Run();


internal static class StartupMemory
{
    public static string? IndexHtml { get; set; }
}
