using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Api
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

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "AwesomeCMDB API",
                    Description = "ASP.NET Core Web API for interacting with Awesome CMDB",
                    Contact = new OpenApiContact
                    {
                        Name = "Mark Richardson",
                        Email = string.Empty,
                        Url = new Uri("https://github.com/SleepyFoxStudio"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under GNU General Public License",
                        Url = new Uri("https://example.com/license"),
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });



            services.Configure<AccessKeyAuthenticationOptions>(AccessKeyAuthenticationHandler.DefaultSchemeName, Configuration.GetSection("Authentication"));
            services.AddAuthentication(AccessKeyAuthenticationHandler.DefaultSchemeName)
                .AddScheme<AccessKeyAuthenticationOptions, AccessKeyAuthenticationHandler>(AccessKeyAuthenticationHandler.DefaultSchemeName, null);
                
            services.AddCors(options =>
            {
                // this defines a CORS policy called "default"
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins("https://localhost:5003")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // adds an authorization policy to make sure the token is for scope 'api1'
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiAuthorization", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    //policy.RequireClaim("scope", "api1");
                });
            });
        }

        public void Configure(IApplicationBuilder app)
        {

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Awesome CMDB V1");
            });


            app.UseRouting();

            app.UseCors("default");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireAuthorization("ApiAuthorization");
            });
        }
    }

    public static class AccessKeyIdentityExtensions
    {
        public static string GetAccountId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(AccessKeyAuthenticationHandler.AccountIdClaimName);
        }
    }

    public class AccessKeyAuthenticationHandler : AuthenticationHandler<AccessKeyAuthenticationOptions>
    {
        public static string AccountIdClaimName { get; } = "AccountId";

        public static string DefaultSchemeName { get; } = "AccessKey";

        public static string AuthenticationType { get; } = "AccessKey";

        private static readonly string AccessKeyHeaderName  = "AccessKey";
        private static readonly string AccessSecretHeaderName = "AccessSecret";



        public AccessKeyAuthenticationHandler(IOptionsMonitor<AccessKeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(AccessKeyHeaderName, out StringValues accessKey) && !Options.Bypass)
            {
                return Task.FromResult(AuthenticateResult.Fail($"{AccessKeyHeaderName} header missing."));
            }

            if (!Request.Headers.TryGetValue(AccessSecretHeaderName, out StringValues accessSecret) && !Options.Bypass)
            {
                return Task.FromResult(AuthenticateResult.Fail($"{AccessSecretHeaderName} header missing."));
            }

            if ((accessKey != Options.StaticAccessKey || accessSecret != Options.StaticAccessSecret) && !Options.Bypass)
            {
                return Task.FromResult(AuthenticateResult.Fail("The access key or secret is not correct."));
            }
            
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new [] { new Claim(AccountIdClaimName, Options.StaticAccountId) }, AuthenticationType));

            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
        }
    }

    public class AccessKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public bool Bypass { get; set; }
        public string StaticAccessKey { get; set; }
        public string StaticAccessSecret { get; set; }
        public string StaticAccountId { get; set; }
    }
}