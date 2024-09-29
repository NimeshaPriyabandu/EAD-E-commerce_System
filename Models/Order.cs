using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace E_commerce_system.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString(); // Unique Order ID

        public string ProductId { get; set; } = string.Empty;    // ID of the purchased product
        public string CustomerId { get; set; } = string.Empty;   // ID of the customer who placed the order
        public int Quantity { get; set; } = 1;                   // Quantity of the product
        public decimal TotalPrice { get; set; } = 0;             // Total price of the order

        public string Status { get; set; } = "Processing";       // Order status: Processing, Dispatched, Delivered, Cancelled

        public DateTime OrderDate { get; set; } = DateTime.UtcNow; // Date of order placement
        public DateTime? DispatchedDate { get; set; }            // Date of order dispatch
        public DateTime? DeliveryDate { get; set; }              // Date of order delivery
    }
}
