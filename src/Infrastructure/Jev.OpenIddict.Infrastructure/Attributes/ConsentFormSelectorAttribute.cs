using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;

namespace Jev.OpenIddict.Infrastructure.Attributes
{
    public class ConsentFormSelectorAttribute : ActionMethodSelectorAttribute
    {
        private readonly string[] _names;

        public ConsentFormSelectorAttribute(params string[] names)
        {
            _names = names;
        }

        public override bool IsValidForRequest(RouteContext context, ActionDescriptor action)
        {
            if (string.Equals(context.HttpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(context.HttpContext.Request.Method, "HEAD", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(context.HttpContext.Request.Method, "DELETE", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(context.HttpContext.Request.Method, "TRACE", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (string.IsNullOrEmpty(context.HttpContext.Request.ContentType))
            {
                return false;
            }

            if (!context.HttpContext.Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return _names.Where(name => !string.IsNullOrEmpty(context.HttpContext.Request.Form[name])).Count() > 0;
        }
    }
}
