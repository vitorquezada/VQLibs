using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using VQLib.Jwt;
using VQLib.Jwt.Model;

namespace VQLib.Api.Configuration
{
    public static class VQJwtConfiguration
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var jwtDescriptor = new VQJwtDescriptor();
            configuration.GetSection("JWTDescriptor").Bind(jwtDescriptor);

            var secretKey = configuration.GetSection("JWTSecretKey").Value;

            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = secretKey.GetSymmetricSecurityKey(),
                        ValidateIssuer = true,
                        ValidIssuer = jwtDescriptor.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtDescriptor.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                });
        }
    }
}