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

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] User user)
        {
            try
            {
                // Validate input fields
                if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.PasswordHash) || string.IsNullOrEmpty(user.Role))
                {
                    return BadRequest(new { message = "Email, password, and role are required." });
                }

                // Check if the email format is valid (basic validation)
                if (!IsValidEmail(user.Email))
                {
                    return BadRequest(new { message = "Invalid email format." });
                }

                // Check if the user already exists
                var existingUser = await _userService.GetUserByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "User already exists." });
                }

                // Hash the password
                user.PasswordHash = _userService.HashPassword(user.PasswordHash);

                Console.WriteLine($"Hashed password for {user.Email}: {user.PasswordHash}");

                if (user.Role == "Vendor")
                {
                    // Create a new Vendor
                    var vendor = new Vendor
                    {
                        Email = user.Email,
                        Name=user.Name,
                        PhoneNumber=user.PhoneNumber,
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
                    // Create a regular user
                    await _userService.CreateUserAsync(user);
                    return Ok(new { message = "User created successfully." });
                }
            }
            catch (Exception ex)
            {
                // Log the error (optional)
                Console.WriteLine($"Error occurred during signup: {ex.Message}");

                // Return internal server error with the exception message
                return StatusCode(500, new { message = "An error occurred while creating the user.", error = ex.Message });
            }
        }

        // Helper method to validate email format
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
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
                // Check if the user exists by email
                var user = await _userService.GetUserByEmailAsync(loginRequest.Email);
                if (user == null)
                {
                    // Email does not exist
                    return BadRequest(new { message = "User with the given email does not exist. Please contact the administrator." });
                }

                // Log the found user details
                Console.WriteLine($"User found: {user.Email}, Role: {user.Role}");

                // Hash the input password
                var hashOfInput = _userService.HashPassword(loginRequest.Password);
                Console.WriteLine($"Hashed input password: {hashOfInput}");
                Console.WriteLine($"Stored password hash: {user.PasswordHash}");

                // Check if the hashed input matches the stored password hash
                if (hashOfInput != user.PasswordHash)
                {
                    // Incorrect password
                    Console.WriteLine("Invalid password for user: " + user.Email);
                    return BadRequest(new { message = "Incorrect password. Please try again." });
                }

                // Check if the user account is inactive
                if (!user.IsActive)
                {
                    // Account is inactive
                    return BadRequest(new { message = "Your account is inactive. Please contact the administrator." });
                }

                // Generate JWT token if all checks pass
                var token = GenerateJwtToken(user);

                // Return token if login is successful
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                // Return a generic error message for any unexpected errors
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

        [HttpPut("admin/update-profile/{userId}")]
        public async Task<IActionResult> AdminUpdateUserProfile(string userId, [FromBody] UpdateUserProfileDto updatedProfile)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "User ID is required." });
            }

            var result = await _userService.UpdateUserProfileAsync(userId, updatedProfile);
            
            if (!result)
            {
                return NotFound(new { message = "User not found or update failed." });
            }

            return Ok(new { message = "User profile updated successfully." });
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
