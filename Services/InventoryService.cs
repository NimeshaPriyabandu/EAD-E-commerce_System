// -----------------------------------------------------------------------------
// InventoryService.cs
// 
// This service class provides functionality for managing inventory in the 
// e-commerce system. It supports operations such as checking, updating, 
// reserving, and releasing stock, as well as monitoring inventory levels 
// for reorder alerts and managing vendor notifications.
// -----------------------------------------------------------------------------

using E_commerce_system.Configurations;
using E_commerce_system.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using E_commerce_system.Services;

namespace E_commerce_system.Services
{
    public class InventoryService
    {
        private readonly IMongoCollection<Inventory> _inventory;
        private readonly ProductService _productService;

        // Constructor to initialize MongoDB connection and collection.
        public InventoryService(IOptions<MongoDBSettings> mongoDBSettings,ProductService productService)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _inventory = database.GetCollection<Inventory>("Inventory");
            _productService = productService;
        }

        // Get stock items for a specific vendor.
        public List<Inventory> GetStocksByVendor(string vendorId)
        {
            var vendorStocks = _inventory.Find(i => i.VendorId == vendorId).ToList();
            return vendorStocks;
        }

        public List<Inventory> GetAllInventory()
        {
            return _inventory.Find(i => true).ToList(); // Get all inventory records.
        }

        // Check if stock is available for a product and vendor.
        public bool CheckStock(string productId, string vendorId, int quantity)
        {
            var inventory = _inventory.Find(i => i.ProductId == productId && i.VendorId == vendorId).FirstOrDefault();
            return inventory != null && inventory.AvailableQuantity >= quantity;
        }

        // Update stock for a specific product and vendor.
        public void UpdateStock(string productId, string vendorId, int quantity)
        {
            var inventory = _inventory.Find(i => i.ProductId == productId && i.VendorId == vendorId).FirstOrDefault();
            
            if (inventory != null)
            {
                // Update existing stock.
                inventory.AvailableQuantity = quantity;
                _inventory.ReplaceOne(i => i.Id == inventory.Id, inventory);
            }
            else
            {
                // Create new inventory record.
                var newInventory = new Inventory
                {
                    ProductId = productId,
                    VendorId = vendorId,
                    AvailableQuantity = quantity,
                    ReservedQuantity = 0,
                    ReorderLevel = 10,
                    Notifications = new List<string>()
                };
                _inventory.InsertOne(newInventory);
            }

            // Check if stock is below reorder level.
            CheckReorderLevel(productId, vendorId);
        }

        // Reserve stock for an order.
        public bool ReserveStock(string productId, string vendorId, int quantity)
        {
            var inventory = _inventory.Find(i => i.ProductId == productId && i.VendorId == vendorId).FirstOrDefault();
            
            if (inventory != null && inventory.AvailableQuantity >= quantity)
            {
                // Reserve stock by reducing available quantity.
                inventory.AvailableQuantity -= quantity;
                inventory.ReservedQuantity += quantity;
                _inventory.ReplaceOne(i => i.Id == inventory.Id, inventory);
                return true; 
            }

            return false;
        }

        // Release reserved stock back to available stock.
        public void ReleaseStock(string productId, string vendorId, int quantity)
        {
            // Find the inventory record for the specific product and vendor.
            var inventory = _inventory.Find(i => i.ProductId == productId && i.VendorId == vendorId).FirstOrDefault();
            
            if (inventory != null && inventory.ReservedQuantity >= quantity)
            {
                // Decrease the reserved quantity as the stock has been delivered.
                inventory.ReservedQuantity -= quantity;

                // Update the inventory record in the database.
                _inventory.ReplaceOne(i => i.Id == inventory.Id, inventory);
            }
        }

        // Check if inventory is below reorder level and generate notifications.
        public void CheckReorderLevel(string productId, string vendorId)
        {
            // Get the product details
            var product = _productService.Get(productId);
            string productName = product != null ? product.Name : "Unknown Product";

            var inventory = _inventory.Find(i => i.ProductId == productId && i.VendorId == vendorId).FirstOrDefault();
            
            if (inventory != null && inventory.AvailableQuantity <= inventory.ReorderLevel)
            {
                // Create low stock alert notification with product name
                string notification = $"Low stock alert: Only {inventory.AvailableQuantity} units left in stock for Product '{productName}'.";

                if (!inventory.Notifications.Contains(notification))
                {
                    inventory.Notifications.Add(notification);
                }

                _inventory.ReplaceOne(i => i.Id == inventory.Id, inventory);
            }
        }

        // Retrieve all notifications for a specific vendor.
        public List<string> GetVendorNotifications(string vendorId)
        {
            var inventories = _inventory.Find(i => i.VendorId == vendorId).ToList();
            var notifications = new List<string>();

            // Collect notifications from all inventory records.
            foreach (var inventory in inventories)
            {
                notifications.AddRange(inventory.Notifications);
            }

            return notifications;
        }
    }
}
