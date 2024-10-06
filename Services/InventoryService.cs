using E_commerce_system.Configurations;
using E_commerce_system.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;

namespace E_commerce_system.Services
{
    public class InventoryService
    {
        private readonly IMongoCollection<Inventory> _inventory;

        public InventoryService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _inventory = database.GetCollection<Inventory>("Inventory");
        }

        public List<Inventory> GetStocksByVendor(string vendorId)
        {
            // Find all inventory records that belong to the specified vendor
            var vendorStocks = _inventory.Find(i => i.VendorId == vendorId).ToList();

            return vendorStocks;
        }

        
        public bool CheckStock(string productId, string vendorId, int quantity)
        {
            var inventory = _inventory.Find(i => i.ProductId == productId && i.VendorId == vendorId).FirstOrDefault();
            return inventory != null && inventory.AvailableQuantity >= quantity;
        }

        public void UpdateStock(string productId, string vendorId, int quantity)
        {
            var inventory = _inventory.Find(i => i.ProductId == productId && i.VendorId == vendorId).FirstOrDefault();
            
            if (inventory != null)
            {
                // If inventory exists, update the available stock
                inventory.AvailableQuantity += quantity;
                _inventory.ReplaceOne(i => i.Id == inventory.Id, inventory);
            }
            else
            {
                // If inventory doesn't exist, create a new one
                var newInventory = new Inventory
                {
                    ProductId = productId,
                    VendorId = vendorId,
                    AvailableQuantity = quantity, // Initialize with the provided quantity
                    ReservedQuantity = 0,         // Initialize reserved quantity as 0
                    ReorderLevel = 10,            // Default reorder level (can be modified)
                    Notifications = new List<string>() // Initialize with an empty notification list
                };

                // Insert the new inventory record into the database
                _inventory.InsertOne(newInventory);
            }

            // After updating or creating the inventory, check if it's below the reorder level
            CheckReorderLevel(productId, vendorId);
        }


        // Reserve stock when an order is placed by a vendor
        public bool ReserveStock(string productId, string vendorId, int quantity)
        {
            var inventory = _inventory.Find(i => i.ProductId == productId && i.VendorId == vendorId).FirstOrDefault();
            
            if (inventory != null && inventory.AvailableQuantity >= quantity)
            {
                // Decrease available stock and increase reserved stock
                inventory.AvailableQuantity -= quantity;
                inventory.ReservedQuantity += quantity;

                // Update the inventory in the database
                _inventory.ReplaceOne(i => i.Id == inventory.Id, inventory);

                return true; // Stock reservation successful
            }

            // If not enough stock or inventory not found, return false
            return false;
        }


        // Release stock if an order is canceled by a vendor
        public void ReleaseStock(string productId, string vendorId, int quantity)
        {
            var inventory = _inventory.Find(i => i.ProductId == productId && i.VendorId == vendorId).FirstOrDefault();
            if (inventory != null && inventory.ReservedQuantity >= quantity)
            {
                inventory.AvailableQuantity += quantity;
                inventory.ReservedQuantity -= quantity;
                _inventory.ReplaceOne(i => i.Id == inventory.Id, inventory);
            }
        }

        public void CheckReorderLevel(string productId, string vendorId)
        {
            var inventory = _inventory.Find(i => i.ProductId == productId && i.VendorId == vendorId).FirstOrDefault();
            
            if (inventory != null && inventory.AvailableQuantity <= inventory.ReorderLevel)
            {
                // Prepare the notification message
                string notification = $"Low stock alert: Only {inventory.AvailableQuantity} units left in stock for Product {productId}.";

                // Add the notification only if it doesn't already exist
                if (!inventory.Notifications.Contains(notification))
                {
                    inventory.Notifications.Add(notification);
                }

                // Update the inventory in the database with the new notification
                _inventory.ReplaceOne(i => i.Id == inventory.Id, inventory);
            }
        }

        public List<string> GetVendorNotifications(string vendorId)
        {
            // Retrieve all inventory items for the vendor
            var inventories = _inventory.Find(i => i.VendorId == vendorId).ToList();
            
            // Collect all notifications
            var notifications = new List<string>();

            foreach (var inventory in inventories)
            {
                notifications.AddRange(inventory.Notifications);
            }

            return notifications;
        }

    }
}
