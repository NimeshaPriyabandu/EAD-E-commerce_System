// -----------------------------------------------------------------------------
// UserService.cs
// 
// This service class handles user management, including creation, retrieval, 
// updating user profiles, and managing activation/deactivation of users. 
// It also includes password hashing and validation mechanisms.
// -----------------------------------------------------------------------------

using E_commerce_system.Models;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace E_commerce_system.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        // Constructor to initialize the users collection.
        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("Users");
        }

        // Get user by email.
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        // Get all users.
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _users.Find(_ => true).ToListAsync(); 
        }

        // Get user by ID.
        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        }

        // Create a new user.
        public async Task<User> CreateUserAsync(User user)
        {
            await _users.InsertOneAsync(user); 
            return user;
        }

        // Validate user credentials.
        public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null) return false;

            return VerifyPassword(password, user.PasswordHash);
        }

        // Update user information.
        public async Task UpdateUserAsync(User user)
        {
            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
        }

        // Activate user by ID.
        public async Task<bool> ActivateUserAsync(string userId)
        {
            return await SetUserActivationStatusAsync(userId, true);
        }

        // Deactivate user by ID.
        public async Task<bool> DeactivateUserAsync(string userId)
        {
            return await SetUserActivationStatusAsync(userId, false);
        }

        // Set user activation status.
        private async Task<bool> SetUserActivationStatusAsync(string userId, bool isActive)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return false; 
            }

            user.IsActive = isActive;
            await UpdateUserAsync(user);
            return true; 
        }

        // Get all customers.
        public async Task<List<User>> GetAllCustomersAsync()
        {
            var filter = Builders<User>.Filter.Eq(u => u.Role, "Customer");
            return await _users.Find(filter).ToListAsync();
        }

        // Hash password using SHA-256.
        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Verify password.
        public bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }

        // Update user profile (name and phone number).
        public async Task<bool> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updatedProfile)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return false; 
            }

            // Update fields only if new values are provided.
            user.Name = updatedProfile.Name ?? user.Name;
            user.PhoneNumber = updatedProfile.PhoneNumber ?? user.PhoneNumber;

            await UpdateUserAsync(user);
            return true; 
        }
    }

    // DTO for updating user profile.
    public class UpdateUserProfileDto
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
    }
}
