using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace E_commerce_system.Models
{
    public class Vendor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string VendorId { get; set; } = ObjectId.GenerateNewId().ToString();

        public string UserId { get; set; } // Link to the User model (who is a Vendor)

        public List<CustomerRating> Ratings { get; set; } = new List<CustomerRating>();
        public double AverageRating { get; set; } // Automatically calculated from Ratings
    }

    public class CustomerRating
    {
        public string CustomerId { get; set; } // UserId of the customer who gave the rating
        public int Rating { get; set; } // Rating value (e.g., 1 to 5 stars)
        public string Comment { get; set; } = string.Empty; // Optional comment
    }
}
