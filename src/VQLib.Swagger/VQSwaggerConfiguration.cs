using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace VQLib.Swagger
{
    public static class VQSwaggerConfiguration
    {
        public const string NAME = "API Moneytoria";

        public static void ConfigureSwaggerService(IServiceCollection services, Action<SwaggerGenOptions> func = null)
        {
            services.AddSwaggerGen(c =>
            {
                c.IgnoreObsoleteProperties();
                c.IgnoreObsoleteActions();

                var scheme = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                };

                c.AddSecurityDefinition("Bearer", scheme);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { scheme, Array.Empty<string>() }
                });

                if (func != null)
                    func(c);
            });
        }

        public static void ConfigureSwaggerApp(IApplicationBuilder app, Action<SwaggerUIOptions> funcUI = null, Action<SwaggerOptions> func = null)
        {
            app.UseSwagger(c =>
            {
                if (func != null)
                    func(c);
            });
            app.UseSwaggerUI(c =>
            {
                //c.SwaggerEndpoint("/swagger/v1/swagger.json", NAME);
                //c.RoutePrefix = "swagger";
                //c.DocumentTitle = NAME;
                c.DocExpansion(DocExpansion.None);

                if (funcUI != null)
                    funcUI(c);
            });
        }
    }
}