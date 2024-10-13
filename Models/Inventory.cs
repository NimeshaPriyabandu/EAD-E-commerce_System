// -----------------------------------------------------------------------------
// Inventory.cs
// 
// Represents the inventory for a product. This model tracks the available and 
// reserved quantities for a product, as well as the reorder level and any 
// notifications related to low stock. Each inventory record is tied to a 
// specific product and vendor.
// -----------------------------------------------------------------------------

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace E_commerce_system.Models
{
    public class Inventory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString(); // Inventory ID

        public string ProductId { get; set; } = string.Empty;  // Product ID
        public string VendorId { get; set; } = string.Empty;   // Vendor ID
        public int AvailableQuantity { get; set; }             // Available quantity of product
        public int ReservedQuantity { get; set; } = 0;         // Reserved quantity of product
        public int ReorderLevel { get; set; }                  
        public List<string> Notifications { get; set; } = new List<string>(); // Notifications list
    }
}
