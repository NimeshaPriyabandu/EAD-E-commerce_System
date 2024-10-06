using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace E_commerce_system.Models
{
    public class CartItem
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; } // Price at the time of adding to the cart
    }

    public class Cart
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } // Cart belongs to a user

        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal TotalPrice { get; set; } = 0; // Automatically calculated
    }
}
