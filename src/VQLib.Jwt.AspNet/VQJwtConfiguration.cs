using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using VQLib.Jwt.Model;

namespace VQLib.Jwt.AspNet
{
    public static class VQJwtConfiguration
    {
        public static void ConfigureServices(IServiceCollection services, VQJwtDescriptor jwtDescriptor)
        {
            services
                .AddAuthentication(x =>
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
                        IssuerSigningKey = jwtDescriptor.SecretKey.GetSymmetricSecurityKey(),
                        ValidateIssuer = true,
                        ValidIssuer = jwtDescriptor.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtDescriptor.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30),
                    };
                });
        }
    }
}