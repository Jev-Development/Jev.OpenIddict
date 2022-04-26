using Jev.OpenIddict.Domain.Dto;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jev.OpenIddict.Infrastructure.Middleware
{
    public class ErrorResponseMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorResponseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentException ex)
            {
                await WriteErrorResponse(context, ex, 400);
            }
            catch (SecurityException ex)
            {
                await WriteErrorResponse(context, ex, 401);
            }
            catch (Exception ex)
            {
                await WriteErrorResponse(context, ex, 400);
            }

        }

        private static async Task WriteErrorResponse(HttpContext context, Exception ex, int statusCode)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ErrorResponseDto()
            {
                Message = ex.Message,
                ResponseCode = statusCode
            }, options: new JsonSerializerOptions()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }

    public static class ErrorResponseMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorResponseMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorResponseMiddleware>();
        }
    }
}
