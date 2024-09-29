using E_commerce_system.Configurations;
using E_commerce_system.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace E_commerce_system.Services
{
    public class OrderService
    {
        private readonly IMongoCollection<Order> _orders;

        public OrderService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _orders = database.GetCollection<Order>("Orders");
        }

        // Get all orders
        public List<Order> Get() => _orders.Find(order => true).ToList();

        // Get a specific order by ID
        public Order Get(string id) => _orders.Find<Order>(order => order.Id == id).FirstOrDefault();

        // Create a new order
        public Order Create(Order order)
        {
            order.OrderDate = DateTime.UtcNow; // Set the order date to now
            _orders.InsertOne(order);
            return order;
        }

        // Update order details (only if status is "Processing")
        public bool Update(string id, Order orderIn)
        {
            var order = _orders.Find<Order>(o => o.Id == id && o.Status == "Processing").FirstOrDefault();
            if (order == null)
            {
                return false; // Order not found or not in "Processing" state
            }

            orderIn.Id = order.Id;
            _orders.ReplaceOne(o => o.Id == id, orderIn);
            return true;
        }

        // Cancel an order (only if status is "Processing")
        public bool CancelOrder(string id)
        {
            var order = _orders.Find<Order>(o => o.Id == id && o.Status == "Processing").FirstOrDefault();
            if (order == null)
            {
                return false; // Order not found or not in "Processing" state
            }

            _orders.UpdateOne(order => order.Id == id, Builders<Order>.Update.Set(o => o.Status, "Cancelled"));
            return true;
        }

        // Update order status (e.g., Dispatch, Deliver)
        public bool UpdateOrderStatus(string id, string newStatus)
        {
            var order = _orders.Find<Order>(o => o.Id == id).FirstOrDefault();
            if (order == null || order.Status == "Cancelled" || order.Status == "Delivered")
            {
                return false; // Cannot update status for non-existing, cancelled, or delivered orders
            }

            var updateDefinition = Builders<Order>.Update.Set(o => o.Status, newStatus);

            if (newStatus == "Dispatched")
            {
                updateDefinition = updateDefinition.Set(o => o.DispatchedDate, DateTime.UtcNow);
            }
            else if (newStatus == "Delivered")
            {
                updateDefinition = updateDefinition.Set(o => o.DeliveryDate, DateTime.UtcNow);
            }

            _orders.UpdateOne(order => order.Id == id, updateDefinition);
            return true;
        }
    }
}
