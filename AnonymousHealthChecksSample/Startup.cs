using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AnonymousHealthChecksSample.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace AnonymousHealthChecksSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddHealthChecks();

            // Don't do this in real code. This validates nothing in the token and is there for basic demo purposes
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateLifetime = false,
                        ValidateTokenReplay = false,
                        ValidateIssuerSigningKey = false,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        RequireSignedTokens = false
                    };
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // This is here for demo purposes to stick a token on the incoming request. Ignore it!
            // This is not required in real-world apps as the token should already be in the header.
            app.Use(async (ctx, next) =>
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new []
                    {
                        new Claim("Tenant", "ABC123")
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(5)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);

                ctx.Request.Headers.Add(HeaderNames.Authorization, $"Bearer {tokenHandler.WriteToken(token)}");

                await next();
            });

            // Below is the code we care about!

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseRouteChecker();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthcheck").WithMetadata(new AllowAnonymousAttribute());
                endpoints.MapControllers().RequireAuthorization();
            });
        }
    }
}
