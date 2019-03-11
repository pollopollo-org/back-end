

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PolloPollo.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PolloPollo.Web.Security
{
    public class JwtTokenProvider
    {
        /// <summary>
        /// Method that generates a JSON Web Token for a given user, that should be used
        /// to authorize subsequent requests
        /// </summary>
        /// <param name="user">An entity of the user that attempts to generate a token</param>
        /// <returns></returns>
        public static string GenerateAccessToken(DummyEntity user)
        {
            var config = Program.Configuration;

            var jwtSecurityToken = new JwtSecurityToken
            (
                issuer: config["Authentication:AppDomain"],
                audience: config["Authentication:AppDomain"],
                claims: CreateClaims(user),
                expires: DateTime.UtcNow.Add(TimeSpan.FromDays(7)),
                signingCredentials: config["Authentication:Secret"].ToIdentitySigningCredentials()
            );

            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return token;
        }

        public static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                var config = Program.Configuration;
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return null;

                var symmetricKey = config["Authentication:Secret"].ToSymmetricSecurityKey();

                var validationParameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = symmetricKey
                };

                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);

                return principal;
            }

            catch (Exception)
            {
                //should write log
                return null;
            }
        }

        /// <summary>
        /// Internal helper that generates the claims required to generate a token
        /// for the passed user
        /// </summary>
        /// <param name="user">An entity of the user to create the claims of</param>
        /// <returns></returns>
        private static IEnumerable<Claim> CreateClaims(DummyEntity user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                //new Claim(ClaimTypes.Name, user.Username),
            };

            //if (user.Roles != null)
            //{
            //    foreach (var role in user.Roles)
            //    {
            //        claims.Add(new Claim(ClaimTypes.Role, role));
            //    }
            //}

            return claims;
        }
    }
}
