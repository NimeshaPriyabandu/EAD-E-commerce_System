// -----------------------------------------------------------------------------
// Vendor.cs
// 
// Represents a vendor in the e-commerce system, inheriting from the User class. 
// Vendors can receive ratings and comments from customers. This model tracks 
// the list of customer ratings and calculates the average rating for the vendor.
// -----------------------------------------------------------------------------

// -----------------------------------------------------------------------------
// CustomerRating.cs
// 
// Represents a customer rating for a vendor. Contains the customer ID, rating 
// score, comment, and an optional reference to the customer who left the rating.
// -----------------------------------------------------------------------------

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace E_commerce_system.Models
{
    [BsonDiscriminator("Vendor")] 
    public class Vendor : User
    {
        public List<CustomerRating> Ratings { get; set; } = new List<CustomerRating>(); 

        public double AverageRating { get; set; } = 0.0; 
    }

    public class CustomerRating
    {
        public string CustomerId { get; set; } = string.Empty; 
        public int Rating { get; set; } = 0; 
        public string Comment { get; set; } = string.Empty; 
        public User? Customer { get; set; }
    }
}
