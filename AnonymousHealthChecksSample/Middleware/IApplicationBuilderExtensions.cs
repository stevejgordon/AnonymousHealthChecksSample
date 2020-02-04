using System;
using Microsoft.AspNetCore.Builder;

namespace AnonymousHealthChecksSample.Middleware
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRouteChecker(this IApplicationBuilder app)
        {
            _ = app ?? throw new ArgumentNullException(nameof(app));

            app.UseMiddleware<CustomRouteCheckerMiddleware>();

            return app;
        }
    }
}
