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

        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            // Extract the user ID from the JWT token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            // Retrieve the user by their ID
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(user);
        }

        // New endpoint to get all users (Only for Administrators)
        [HttpGet("users")] 
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                Console.WriteLine("Fetching all users...");

                var users = await _userService.GetAllCustomersAsync();
                if (users.Count == 0)
                {
                    Console.WriteLine("No users found.");
                    return NotFound(new { message = "No users found." });
                }


                Console.WriteLine($"Found {users.Count} users.");
                return Ok(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching users.", error = ex.Message });
            }
        }

        // Endpoint to activate or deactivate a user account (Administrator only)
        [HttpPut("users/{id}/activate")]
        [Authorize(Roles = "Administrator")] // Only admins can activate/deactivate users
        public async Task<IActionResult> ActivateDeactivateUser(string id, [FromBody] bool isActive)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                user.IsActive = isActive;
                await _userService.UpdateUserAsync(user);

                var status = isActive ? "activated" : "deactivated";
                Console.WriteLine($"User {user.Email} has been {status}.");
                return Ok(new { message = $"User {status} successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during account activation/deactivation: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while updating the user status.", error = ex.Message });
            }
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] User user)
        {
            try
            {
                // Check if the user already exists
                if (await _userService.GetUserByEmailAsync(user.Email) != null)
                {
                    return BadRequest(new { message = "User already exists." });
                }

                // Hash the user's password before saving
                user.PasswordHash = _userService.HashPassword(user.PasswordHash);

                // Log the hashed password for debugging
                Console.WriteLine($"Hashed password for {user.Email}: {user.PasswordHash}");

                if (user.Role == "Vendor")
                {
                    var vendor = new Vendor
                    {
                        Email = user.Email,
                        PasswordHash = user.PasswordHash,
                        Role = "Vendor",
                        IsActive = true,
                        Ratings = new List<CustomerRating>(),
                        AverageRating = 0.0
                    };

                    await _userService.CreateUserAsync(vendor);
                    return Ok(new { message = "Vendor created successfully." });
                }
                else
                {
                    await _userService.CreateUserAsync(user);
                    return Ok(new { message = "User created successfully." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the user.", error = ex.Message });
            }
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                // Retrieve user by email
                var user = await _userService.GetUserByEmailAsync(loginRequest.Email);
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid credentials." });
                }

                // Log user details for debugging
                Console.WriteLine($"User found: {user.Email}, Role: {user.Role}");

                // Hash the input password and compare it with the stored hash
                var hashOfInput = _userService.HashPassword(loginRequest.Password);
                Console.WriteLine($"Hashed input password: {hashOfInput}");
                Console.WriteLine($"Stored password hash: {user.PasswordHash}");

                if (hashOfInput != user.PasswordHash)
                {
                    Console.WriteLine("Invalid password for user: " + user.Email);
                    return Unauthorized(new { message = "Invalid credentials." });
                }

                if (!user.IsActive)
                {
                    return Forbid("Account is not activated.");
                }

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
