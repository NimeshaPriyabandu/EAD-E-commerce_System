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
            order.Status = "Processing"; 
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

        public List<Order> GetOrdersByCustomerId(string customerId)
        {
            return _orders.Find(order => order.CustomerId == customerId).ToList();
        }

        // Customer requests cancellation; CSR approves/rejects it
    public string RequestOrderCancellation(string orderId)
    {
        var order = GetById(orderId);
        if (order == null)
        {
            return "Order not found.";
        }

        // Add the cancellation request directly to the order (or use some flag)
        if (order.Status != "Processing")
        {
            return "Only 'Processing' orders can be canceled.";
        }

        order.Status = "Cancellation Requested"; // Set order status to 'Cancellation Requested'
        // Optionally, save the reason for cancellation as a field
        order.UpdatedAt = DateTime.UtcNow;

        _orders.ReplaceOne(o => o.Id == orderId, order);
        return "Cancellation requested successfully.";
    }

    // CSR processes the cancellation request
    public string ProcessCancellationRequest(string orderId, string action)
    {
        var order = GetById(orderId);
        if (order == null)
        {
            return "Order not found.";
        }

        if (order.Status != "Cancellation Requested")
        {
            return "No cancellation request found for this order.";
        }

        if (action == "Approve")
        {
            order.Status = "Cancelled"; // Mark order as canceled
        }
        else if (action == "Reject")
        {
            order.Status = "Processing"; // Revert order back to processing if rejected
        }
        else
        {
            return "Invalid action. Must be 'Approve' or 'Reject'.";
        }

        order.UpdatedAt = DateTime.UtcNow;
        _orders.ReplaceOne(o => o.Id == orderId, order);
        return $"Order {action.ToLower()} successfully.";
    }

    public List<Order> GetAllCancellationRequests()
    {
        return _orders.Find(order => order.Status == "Cancellation Requested").ToList();
    }
    
    }
}
