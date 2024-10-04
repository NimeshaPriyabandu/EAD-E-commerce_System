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
        public Order Create(Order order)
        {
            order.TotalPrice = CalculateTotalPrice(order.Items);
            _orders.InsertOne(order);
            return order;
        }

        // Update an existing order
        public void Update(string id, Order updatedOrder)
        {
            updatedOrder.UpdatedAt = DateTime.UtcNow;
            updatedOrder.TotalPrice = CalculateTotalPrice(updatedOrder.Items);
            _orders.ReplaceOne(order => order.Id == id, updatedOrder);
        }

        // Cancel an order by updating the status
        public void CancelOrder(string id)
        {
            var update = Builders<Order>.Update.Set(o => o.Status, "Cancelled")
                                               .Set(o => o.UpdatedAt, DateTime.UtcNow);
            _orders.UpdateOne(order => order.Id == id, update);
        }

        // Update order status (e.g., Shipped, Delivered)
        public void UpdateOrderStatus(string id, string newStatus)
        {
            var update = Builders<Order>.Update.Set(o => o.Status, newStatus)
                                               .Set(o => o.UpdatedAt, DateTime.UtcNow);
            _orders.UpdateOne(order => order.Id == id, update);
        }

        // Calculate the total price of an order based on the items
        private decimal CalculateTotalPrice(List<OrderItem> items)
        {
            return items.Sum(item => item.TotalPrice);
        }
    }
}
