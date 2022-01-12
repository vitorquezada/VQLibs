using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace VQLib.Api.Configuration
{
    public static class VQSwaggerConfiguration
    {
        public const string NAME = "API Moneytoria";

        public static void ConfigureSwaggerService(IServiceCollection services, List<OpenApiInfo> infos)
        {
            throw new NotImplementedException();

            services.AddSwaggerGen(c =>
            {
                for (var i = 1; i <= infos.Count; i++)
                {
                    c.SwaggerDoc($"v{i}", infos[i - 1]);
                }

                //c.SwaggerDoc("v1", new OpenApiInfo
                //{
                //    Version = "v1",
                //    Title = NAME,
                //    Description = NAME,
                //    Contact = new OpenApiContact
                //    {
                //        Name = "Contato Moneytoria",
                //        Email = "contato@moneytoria.com",
                //        Url = new Uri("https://www.moneytoria.com"),
                //    },
                //});

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
            });
        }

        public static void ConfigureSwaggerApp(IApplicationBuilder app)
        {
            throw new NotImplementedException();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", NAME);
                c.RoutePrefix = "swagger";
                c.DocumentTitle = NAME;
                c.DocExpansion(DocExpansion.None);
            });
        }
    }
}