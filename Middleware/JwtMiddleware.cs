using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims; // Add this line for ClaimTypes
using System.Text;
using System.Threading.Tasks;

namespace E_commerce_system.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        
        // Hardcoded JWT settings for testing
        private readonly string jwtKey = "this_is_a_very_strong_and_secure_key_for_jwt_auth";  // Hardcoded key
        private readonly string jwtIssuer = "your_hardcoded_issuer";
        private readonly string jwtAudience = "your_hardcoded_audience";

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            

            // Extract token from the Authorization header
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            // Debug statement: Check if token is present
            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"Token found in Authorization header: {token}");
                AttachUserToContext(context, token);
            }
            else
            {
                Console.WriteLine("No token found in Authorization header.");
            }

            await _next(context); // Proceed to the next middleware in the pipeline
        }

        private void AttachUserToContext(HttpContext context, string token)
        {
            try
            {
               
                var tokenHandler = new JwtSecurityTokenHandler();
                
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = false, 
                    ValidateAudience = false, 
                    ClockSkew = TimeSpan.Zero 
                }, out SecurityToken validatedToken);


                Console.WriteLine("Token validated successfully.");

                var jwtToken = (JwtSecurityToken)validatedToken;
                
                var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
                
                var userRole = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;

                if (userId != null && userRole != null)
                {
                    context.Items["User"] = new { Id = userId, Role = userRole };
                    Console.WriteLine("User and role attached to context.");
                }
                else
                {
                    Console.WriteLine("User ID or role is missing in the token.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating token: {ex.Message}");
            }
        }
    }
}
