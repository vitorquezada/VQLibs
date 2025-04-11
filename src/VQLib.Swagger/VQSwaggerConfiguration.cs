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
        public static void ConfigureSwaggerService(
            IServiceCollection services,
            Action<SwaggerGenOptions>? func = null,
            bool securitySchemeBearer = true,
            bool securitySchemeBasic = false)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.IgnoreObsoleteProperties();
                c.IgnoreObsoleteActions();

                c.MapType<DateOnly>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "date",
                    Example = OpenApiAnyFactory.CreateFromJson($"\"{DateTime.UtcNow:yyyy-MM-dd}\"")
                });

                if (securitySchemeBearer)
                {
                    var bearerScheme = new OpenApiSecurityScheme
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

                    c.AddSecurityDefinition("Bearer", bearerScheme);

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        { bearerScheme, Array.Empty<string>() }
                    });
                }

                if (securitySchemeBasic)
                {
                    var basicScheme = new OpenApiSecurityScheme
                    {
                        Name = "Basic",
                        Description = "Please enter your username and password",
                        Type = SecuritySchemeType.Http,
                        Scheme = "basic",
                        In = ParameterLocation.Header,
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Basic"
                        }
                    };
                    c.AddSecurityDefinition("Basic", basicScheme);
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            basicScheme, new List<string>()
                        }
                    });
                }

                if (func != null)
                    func(c);
            });
        }

        public static void ConfigureSwaggerApp(IApplicationBuilder app, Action<SwaggerUIOptions>? funcUI = null, Action<SwaggerOptions>? func = null)
        {
            app.UseSwagger(c =>
            {
                if (func != null)
                    func(c);
            });
            app.UseSwaggerUI(c =>
            {
                c.DocExpansion(DocExpansion.None);

                if (funcUI != null)
                    funcUI(c);
            });
        }
    }
}