using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace AnonymousHealthChecksSample.Middleware
{
    public class CustomRouteCheckerMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomRouteCheckerMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() is object)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path;

            var pathParts = path.Value.TrimStart('/').Split('/');

            if (pathParts.Length < 2)
            {
                // If we don't have enough path parts (at least 2) after removing the we return a 404 - not found
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            var tenant = context.User.Claims.FirstOrDefault(x => x.Type == "Tenant");

            if (tenant is object)
            {
                var isPermitted = pathParts[0] == tenant.Value;

                if (isPermitted)
                {
                    await _next(context);
                    return;
                }
            }

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
    }
}
