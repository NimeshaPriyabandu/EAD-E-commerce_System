// -----------------------------------------------------------------------------
// AuthController.cs
// 
// This controller handles user authentication and profile management. It 
// provides endpoints for user signup, login, profile retrieval, and updates. 
// JWT tokens are generated for login and used to authorize profile updates.
// -----------------------------------------------------------------------------

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

        // Constructor to initialize user service and configuration.
        public AuthController(UserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        // Get user profile by user ID from JWT token.
        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(user);
        }

        // Get all users.
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                Console.WriteLine("Fetching all users...");

                var users = await _userService.GetAllUsersAsync();
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

        // Get all customers.
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

        // Register a new user (or vendor).
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] User user)
        {
            try
            {
                if (await _userService.GetUserByEmailAsync(user.Email) != null)
                {
                    return BadRequest(new { message = "User already exists." });
                }
                user.PasswordHash = _userService.HashPassword(user.PasswordHash);

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

        // Update user profile.
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDto updatedProfile)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var result = await _userService.UpdateUserProfileAsync(userId, updatedProfile);
            
            if (!result)
            {
                return NotFound(new { message = "User not found or update failed." });
            }

            return Ok(new { message = "Profile updated successfully." });
        }

        // Login a user and generate JWT token.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
               
                var user = await _userService.GetUserByEmailAsync(loginRequest.Email);
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid credentials." });
                }

                Console.WriteLine($"User found: {user.Email}, Role: {user.Role}");

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

        // Activate a user account.
        [HttpPut("users/{id}/activate")]
        public async Task<IActionResult> ActivateUser(string id)
        {
            var result = await _userService.ActivateUserAsync(id);
            if (!result)
            {
                return NotFound(new { message = "User not found." });
            }
            return Ok(new { message = "User activated successfully." });
        }


        // Deactivate a user account.
        [HttpPut("users/{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            var result = await _userService.DeactivateUserAsync(id);
            if (!result)
            {
                return NotFound(new { message = "User not found." });
            }
            return Ok(new { message = "User deactivated successfully." });
        }

    

    // Generate JWT token for the user.
    private string GenerateJwtToken(User user)
    {
    var jwtKey = "this_is_a_very_strong_and_secure_key_for_jwt_auth";  

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id), 
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role) 
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        claims: claims, 
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
