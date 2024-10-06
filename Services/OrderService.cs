using E_commerce_system.Models;
using E_commerce_system.Services;
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

        private readonly InventoryService _inventoryService; 

        public OrderService(IMongoDatabase database, ProductService productService, InventoryService inventoryService)
        {
            _orders = database.GetCollection<Order>("Orders");
            _productService = productService;
            _inventoryService = inventoryService; 
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
                // Mark order as canceled
                order.Status = "Cancelled";

                // Release stock for all products in the canceled order
                foreach (var orderItem in order.Items)
                {
                    _inventoryService.ReleaseStock(orderItem.ProductId, orderItem.VendorId, orderItem.Quantity);
                }
            }
            else if (action == "Reject")
            {
                // Revert order back to processing if cancellation is rejected
                order.Status = "Processing";
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

        public List<Order> GetOrdersByVendorId(string vendorId)
        {
            // Fetch orders where any item has the given vendorId
            return _orders.Find(order => order.Items.Any(item => item.VendorId == vendorId)).ToList();
        }

        // Method to get all items related to a specific vendor
        public List<OrderItem> GetItemsByVendorId(string vendorId)
        {
            var vendorItems = new List<OrderItem>();

            // Get all orders that contain items from this vendor
            var orders = GetOrdersByVendorId(vendorId);

            // Extract the items belonging to the vendor from those orders
            foreach (var order in orders)
            {
                var items = order.Items.Where(item => item.VendorId == vendorId).ToList();
                vendorItems.AddRange(items);
            }

            return vendorItems;
        }
        
        public string MarkItemAsDelivered(string orderId, string vendorId, string productId)
        {
            var order = GetById(orderId);
            if (order == null)
            {
                return "Order not found.";
            }

            var orderItem = order.Items.FirstOrDefault(i => i.ProductId == productId && i.VendorId == vendorId);
            if (orderItem == null)
            {
                return "Order item not found for this vendor and product.";
            }

            orderItem.DeliveryStatus = "Delivered";

            // Check if all items are delivered, and update order status accordingly
            if (order.Items.All(i => i.DeliveryStatus == "Delivered"))
            {
                order.Status = "Delivered"; // All items delivered
            }
            else if (order.Items.Any(i => i.DeliveryStatus == "Delivered"))
            {
                order.Status = "Partially Delivered"; // Some items delivered, but not all
            }

            order.UpdatedAt = DateTime.UtcNow;
            _orders.ReplaceOne(o => o.Id == order.Id, order);

            return $"Order item for product {productId} marked as delivered by vendor {vendorId}.";
        }


            
    }
}
