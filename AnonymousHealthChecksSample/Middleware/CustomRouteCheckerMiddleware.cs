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

            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is object)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path;

            var pathParts = path.Value.TrimStart('/').Split('/');

            if (pathParts.Length <= 1 || string.IsNullOrEmpty(pathParts[0]))
            {
                // If we don't have enough path parts (at least 2) after removing the we return a 404 - not found
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            var pathProfileId = pathParts[0];

            var tenant = context.User.Claims.FirstOrDefault(x => x.Type == "Tenant");

            // IMAGE AN EXTERNAL CALL (OR CACHE CHECK) TO VALIDATE THAT THE TENANT IS ALLOCATED TO THE PROFILE

            var isPermitted = pathProfileId == "profile1";

            if (isPermitted)
            {
                await _next(context);
                return;
            }
                
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
    }
}
