using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace E_commerce_system.Models
{
    [BsonDiscriminator("User")] // Add discriminator for User
    [BsonKnownTypes(typeof(Vendor))] // Let MongoDB know that Vendor is a subclass of User
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty; // Store hashed password

        [Required]
        public string Role { get; set; } = "Customer"; // Default role

        public bool IsActive { get; set; } = false; // Default is inactive, needs activation by CSR/Admin

        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiryTime { get; set; }

        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
