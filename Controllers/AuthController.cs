using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt; 
using System.Security.Claims;
using System.Text; 
using E_commerce_system.Models;
using E_commerce_system.Services; 


namespace E_commerce_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        public AuthController(UserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] User user)
        {
            try
            {
                Console.WriteLine("Signup request received"); 
                if (await _userService.GetUserByEmailAsync(user.Email) != null)
                {
                    Console.WriteLine("User already exists."); 
                    return BadRequest(new { message = "User already exists." });
                }

                await _userService.CreateUserAsync(user);
                Console.WriteLine("User created successfully."); 
                return Ok(new { message = "User created successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during signup: {ex.Message}"); 
                return StatusCode(500, new { message = "An error occurred while creating the user.", error = ex.Message });
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(loginRequest.Email);
                if (user == null || !await _userService.ValidateUserCredentialsAsync(loginRequest.Email, loginRequest.Password))
                    return Unauthorized(new { message = "Invalid credentials." });

                if (!user.IsActive)
                    return Forbid("Account is not activated.");

                var token = GenerateJwtToken(user);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing the login request.", error = ex.Message });
            }
        }

    private string GenerateJwtToken(User user)
    {
    var jwtKey = "this_is_a_very_strong_and_secure_key_for_jwt_auth";  // Use only the key

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id), // Use NameIdentifier for User ID (matches validation)
        new Claim(JwtRegisteredClaimNames.Email, user.Email), // User Email
        new Claim(ClaimTypes.Role, user.Role) // User Role
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        claims: claims, // No issuer and audience
        expires: DateTime.Now.AddMinutes(30),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
    }




}


    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
