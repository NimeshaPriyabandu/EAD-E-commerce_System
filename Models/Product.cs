// -----------------------------------------------------------------------------
// Product.cs
// 
// Represents a product in the e-commerce system. Contains product details such 
// as name, description, price, stock quantity, and category. It also tracks 
// whether the product is active, has been purchased, and includes vendor details.
// -----------------------------------------------------------------------------

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace E_commerce_system.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString(); 

        public string Name { get; set; } = string.Empty;        
        public string Description { get; set; } = string.Empty; 
        public decimal Price { get; set; }                      
        public int Stock { get; set; }                         
        public bool IsActive { get; set; } = true;              
        public string ImageUrl { get; set; } = string.Empty;    
        public string Category { get; set; } = string.Empty;    
        public string VendorId { get; set; } = string.Empty;    
        public bool IsPurchased { get; set; } = false;          
    }
}
