using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace E_commerce_system.Models
{
    public class Inventory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString(); // Unique Inventory ID

        public string ProductId { get; set; } = string.Empty;  // Link to Product
        public string VendorId { get; set; } = string.Empty;   // Link to Vendor
        public int AvailableQuantity { get; set; }             // Available stock
        public int ReservedQuantity { get; set; } = 0;         // Reserved stock
        public int ReorderLevel { get; set; }       
        public List<string> Notifications { get; set; } = new List<string>();           
    }
}
