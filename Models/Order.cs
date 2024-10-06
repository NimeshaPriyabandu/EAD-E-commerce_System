using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace E_commerce_system.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string CustomerId { get; set; } = string.Empty; // ID of the customer who placed the order
        public List<OrderItem> Items { get; set; } = new List<OrderItem>(); // List of items in the order
        public decimal TotalPrice { get; set; } // Total price of the order
        public string Status { get; set; } = "Processing"; // Order status (e.g., Processing, Shipped, Delivered, Cancelled)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Order creation date
        public DateTime? UpdatedAt { get; set; } // Last update timestamp
    }

    public class OrderItem
    {
        public string ProductId { get; set; } = string.Empty; // ID of the product
        public string ProductName { get; set; } = string.Empty; // Product name for reference
        public int Quantity { get; set; } // Quantity of the product in the order
        public decimal Price { get; set; } // Price per unit of the product
        public decimal TotalPrice => Quantity * Price; // Total price for this item (Quantity * Price)
    }

}
