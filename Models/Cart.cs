// -----------------------------------------------------------------------------
// CartItem.cs
// 
// Represents an individual item in a user's cart. Contains product details 
// such as product ID, name, quantity, and price at the time of adding the item 
// to the cart.
// -----------------------------------------------------------------------------

// -----------------------------------------------------------------------------
// Cart.cs
// 
// Represents a user's shopping cart. It contains a list of cart items, tracks 
// the total price, and is associated with a specific user.
// -----------------------------------------------------------------------------

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
