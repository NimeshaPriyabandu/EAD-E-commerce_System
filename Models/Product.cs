using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace E_commerce_system.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString(); // Default value

        public string Name { get; set; } = string.Empty;        // Default value
        public string Description { get; set; } = string.Empty; // Default value
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; } = true;

        // OR (Choose one of the two options)
        // public string? Id { get; set; } // Option 2: Make properties nullable
        // public string? Name { get; set; } // Option 2: Make properties nullable
        // public string? Description { get; set; } // Option 2: Make properties nullable
    }
}
