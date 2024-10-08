// -----------------------------------------------------------------------------
// User.cs
// 
// Represents a user in the e-commerce system. This base class includes common 
// user details such as email, password hash, role, active status, and token 
// management. The class also includes personal details such as name and phone 
// number. Inheritance is used to define more specific types of users like vendors.
// -----------------------------------------------------------------------------

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace E_commerce_system.Models
{
    [BsonDiscriminator("User")] 
    [BsonKnownTypes(typeof(Vendor))] 
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty; 

        [Required]
        public string Role { get; set; } = "Customer"; 

        public bool IsActive { get; set; } = false; 

        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiryTime { get; set; }

        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
