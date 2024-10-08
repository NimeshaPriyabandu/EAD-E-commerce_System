// -----------------------------------------------------------------------------
// Order
// 
// Represents an order placed by a customer. Each order contains a list of 
// order items, total price, and status tracking (e.g., Processing, Shipped). 
// Timestamps for order creation and updates are also maintained.
// -----------------------------------------------------------------------------

// -----------------------------------------------------------------------------
// OrderItem
// 
// Represents an item in an order. Each order item contains the product details, 
// quantity, price, and the status of its delivery. It also tracks which vendor 
// is responsible for delivering the product.
// -----------------------------------------------------------------------------

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

        public string CustomerId { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new List<OrderItem>(); 
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Processing"; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        public DateTime? UpdatedAt { get; set; } 
    }

    public class OrderItem
    {
        public string ProductId { get; set; } = string.Empty; 
        public string ProductName { get; set; } = string.Empty; 
        public int Quantity { get; set; } 
        public decimal Price { get; set; } 
        public decimal TotalPrice => Quantity * Price; 
        public string VendorId { get; set; } 

        public string DeliveryStatus { get; set; } = "Pending"; 
    }

}
