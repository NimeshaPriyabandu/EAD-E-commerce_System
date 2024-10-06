using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace E_commerce_system.Models
{
    [BsonDiscriminator("Vendor")]  // Discriminator for Vendor class
    public class Vendor : User
    {
        public List<CustomerRating> Ratings { get; set; } = new List<CustomerRating>(); // List of customer ratings

        public double AverageRating { get; set; } = 0.0; // Calculated from Ratings
    }

    public class CustomerRating
    {
        public string CustomerId { get; set; } = string.Empty; // UserId of the customer who gave the rating
        public int Rating { get; set; } = 0; // Rating value (e.g., 1 to 5 stars)
        public string Comment { get; set; } = string.Empty; // Optional comment
        public User? Customer { get; set; } // Optional customer details
    }
}
