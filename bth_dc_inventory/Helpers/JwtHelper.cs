using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace bth_dc_inventory.Helpers
{
    public static class JwtHelper
    {
        public static string GenerateJwtToken(
            string userId,
            string username,
            string email,
            string role,
            string secretKey,
            string issuer,
            string audience)
        {
            try
            {
                Console.WriteLine($"🔧 Generating JWT token for user: {username}");

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, role),
                    new Claim("jti", Guid.NewGuid().ToString()), // JWT ID
                    new Claim("iat", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(24), // ✅ 24 hours
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature // ✅ Use HmacSha256Signature
                    )
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                Console.WriteLine($"✅ JWT token generated successfully");
                Console.WriteLine($"   Token preview: {tokenString.Substring(0, Math.Min(50, tokenString.Length))}...");
                Console.WriteLine($"   Expires: {tokenDescriptor.Expires}");

                return tokenString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generating JWT token: {ex.Message}");
                throw;
            }
        }

        // ✅ Tambahan: Method untuk validate token secara manual
        public static ClaimsPrincipal? ValidateToken(string token, string secretKey, string issuer, string audience)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Token validation failed: {ex.Message}");
                return null;
            }
        }
    }
}