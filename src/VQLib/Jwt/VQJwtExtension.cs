using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using VQLib.Jwt.Model;
using VQLib.Util;

namespace VQLib.Jwt
{
    public static class VQJwtExtension
    {
        private const int _3_HOURS_IN_MINUTES = 180;

        public static string GenerateToken(this List<KeyValuePair<string, string>> dictionaryClaims, VQJwtDescriptor tokenDescriptor, string secretKey, int minutesToExpire = _3_HOURS_IN_MINUTES)
        {
            var now = DateTime.UtcNow;

            var claims = new ClaimsIdentity(dictionaryClaims.Select(x => new Claim(x.Key, x.Value)));

            var tokenHeader = new JwtSecurityTokenHandler();
            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = tokenDescriptor.Issuer,
                Audience = tokenDescriptor.Audience,
                Subject = claims,
                Expires = now.AddMinutes(minutesToExpire),
                NotBefore = now,
                SigningCredentials = GetCredentials(secretKey)
            };

            var token = tokenHeader.CreateToken(securityTokenDescriptor);
            return tokenHeader.WriteToken(token);
        }

        public static string GetToken(this HttpContext httpContext)
        {
            var token = httpContext?.Request?.Headers?["Authorization"].FirstOrDefault()?.Replace("bearer ", string.Empty, StringComparison.InvariantCultureIgnoreCase);
            if (token.IsNullOrWhiteSpace())
                return null;

            return token;
        }

        public static IEnumerable<Claim> GetClaims(this HttpContext httpContext)
        {
            var token = httpContext.GetToken();
            if (token.IsNullOrWhiteSpace())
                return null;

            return token.GetClaims();
        }

        public static IEnumerable<Claim> GetClaims(this string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            var securityToken = (JwtSecurityToken)handler.ReadToken(token);

            return securityToken.Claims;
        }

        public static SymmetricSecurityKey GetSymmetricSecurityKey(this string secretKey) => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        public static SigningCredentials GetCredentials(this string secretKey) => new SigningCredentials(GetSymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha512Signature);
    }
}