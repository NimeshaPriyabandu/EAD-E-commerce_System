using E_commerce_system.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace E_commerce_system.Services
{
    public class OrderService
    {
        private readonly IMongoCollection<Order> _orders;
        private readonly ProductService _productService;

        public OrderService(IMongoDatabase database, ProductService productService)
        {
            _orders = database.GetCollection<Order>("Orders");
            _productService = productService;
        }

        // Get all orders
        public List<Order> GetAll() => _orders.Find(order => true).ToList();

        // Get a specific order by ID
        public Order GetById(string id) => _orders.Find(order => order.Id == id).FirstOrDefault();

        // Create a new order
        public string Create(Order order)
        {
            order.TotalPrice = CalculateTotalPrice(order.Items);
            _orders.InsertOne(order);
            return "Order created successfully.";
        }

        // Update an existing order
        public string Update(string id, Order updatedOrder)
        {
            var existingOrder = GetById(id);
            if (existingOrder == null)
            {
                return "Order not found.";
            }

            if (existingOrder.Status != "Processing")
            {
                return "Order can only be updated if it is in 'Processing' status.";
            }

            updatedOrder.UpdatedAt = DateTime.UtcNow;
            updatedOrder.TotalPrice = CalculateTotalPrice(updatedOrder.Items);
            _orders.ReplaceOne(order => order.Id == id, updatedOrder);
            return "Order updated successfully.";
        }

        // Cancel an order by updating the status
        public string CancelOrder(string id)
        {
            var existingOrder = GetById(id);
            if (existingOrder == null)
            {
                return "Order not found.";
            }

            if (existingOrder.Status == "Shipped" || existingOrder.Status == "Delivered")
            {
                return "Cannot cancel an order that has already been shipped or delivered.";
            }

            var update = Builders<Order>.Update.Set(o => o.Status, "Cancelled")
                                               .Set(o => o.UpdatedAt, DateTime.UtcNow);
            _orders.UpdateOne(order => order.Id == id, update);
            return "Order cancelled successfully.";
        }

        // Update order status (e.g., Processing, Shipped, Delivered)
        public string UpdateOrderStatus(string id, string newStatus)
        {
            var order = GetById(id);
            if (order == null)
            {
                return "Order not found.";
            }

            // Validate status transitions
            if (newStatus == "Delivered" && order.Status != "Shipped")
            {
                return "Order must be 'Shipped' before it can be marked as 'Delivered'.";
            }

            if (newStatus == "Shipped" && order.Status != "Processing")
            {
                return "Order must be 'Processing' before it can be marked as 'Shipped'.";
            }

            var update = Builders<Order>.Update.Set(o => o.Status, newStatus)
                                               .Set(o => o.UpdatedAt, DateTime.UtcNow);
            _orders.UpdateOne(order => order.Id == id, update);
            return $"Order status updated to '{newStatus}'.";
        }

        // Calculate the total price of an order based on the items
        private decimal CalculateTotalPrice(List<OrderItem> items)
        {
            return items.Sum(item => item.TotalPrice);
        }
    }
}
