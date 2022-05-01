using AngleSharp;
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
builder.Services.RegisterOptions(builder.Configuration);
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

string pathBase = builder.Configuration["AppOptions:BasePath"] ?? "/auth";
app.UsePathBase(pathBase);

app.UseCors("corski");

app.UseErrorResponseMiddleware();
app.UseStaticFiles();
app.UseApiVersioning();

app.Use(async (context, next) =>
{
    string[] passthroughSegments = new string[] { $"/api", $"/connect", $"/.well-known" };
    if (!passthroughSegments.Any(path => context.Request.Path.StartsWithSegments(path)))
    {
        if (string.IsNullOrEmpty(StartupMemory.IndexHtml))
        {
            using (var streamReader = new StreamReader(new FileStream(Path.Combine(app.Environment.WebRootPath, "index.html"), FileMode.Open, FileAccess.Read)))
            {
                var indexHtml = await streamReader.ReadToEndAsync();

                using (var asContext = BrowsingContext.New(Configuration.Default))
                {
                    using (var doc = await asContext.OpenAsync(req => req.Content(indexHtml)))
                    {
                        var baseElement = doc.QuerySelector("base") ?? throw new NullReferenceException("No base element defined in front end.");

                        baseElement.SetAttribute("href", pathBase);
                        using (var writer = new StringWriter())
                        {
                            await doc.ToHtmlAsync(writer);
                            StartupMemory.IndexHtml = writer.ToString();
                        }
                    }
                }
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
