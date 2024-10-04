using E_commerce_system.Models;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace E_commerce_system.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("Users");
        }

        // Retrieves a user by email; this can return a User or a Vendor (since Vendor inherits from User)
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        // Retrieves a user by their ID (for getting account details)
        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        }

        // Creates a user or vendor in the collection
        public async Task<User> CreateUserAsync(User user)
        {
            // Insert the user (polymorphic behavior will be handled by MongoDB with discriminators)
            await _users.InsertOneAsync(user); 
            return user;
        }

        // Validates user credentials by comparing the password hash
        public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null) return false;

            // Check if the password matches
            return VerifyPassword(password, user.PasswordHash);
        }

        // Updates a user or vendor's information
        public async Task UpdateUserAsync(User user)
        {
            // Use ReplaceOneAsync to update the user in the collection
            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
        }

        // Activate or deactivate a customer account by Administrator
        public async Task<bool> SetUserActivationStatusAsync(string userId, bool isActive)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return false; // User not found
            }

            user.IsActive = isActive;
            await UpdateUserAsync(user);
            return true; // Successfully updated
        }

        // Get only customers
        public async Task<List<User>> GetAllCustomersAsync()
        {
            var filter = Builders<User>.Filter.Eq(u => u.Role, "Customer");
            return await _users.Find(filter).ToListAsync(); // Fetch only users with Role = "Customer"
        }

        // Hash the password using SHA-256
        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Verifies the input password against the stored hash
        public bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }
    }
}
