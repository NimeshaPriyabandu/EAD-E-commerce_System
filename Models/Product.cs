using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace E_commerce_system.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString(); // Unique Product ID

        public string Name { get; set; } = string.Empty;        // Product Name
        public string Description { get; set; } = string.Empty; // Product Description
        public decimal Price { get; set; }                      // Product Price
        public int Stock { get; set; }                          // Stock Quantity
        public bool IsActive { get; set; } = true;              // Activation Status
        public string ImageUrl { get; set; } = string.Empty;    // URL for the product image
        public string Category { get; set; } = string.Empty;    // Product Category
        public string VendorId { get; set; } = string.Empty;    
        public bool IsPurchased { get; set; } = false;          
    }
}
