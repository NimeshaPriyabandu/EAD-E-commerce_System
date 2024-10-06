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

        // Check stock availability for a product by a specific vendor
        public bool CheckStock(string productId, string vendorId, int quantity)
        {
            var inventory = _inventory.Find(i => i.ProductId == productId && i.VendorId == vendorId).FirstOrDefault();
            return inventory != null && inventory.AvailableQuantity >= quantity;
        }

        // Update stock for a product by a specific vendor
        public void UpdateStock(string productId, string vendorId, int quantity)
        {
            var inventory = _inventory.Find(i => i.ProductId == productId && i.VendorId == vendorId).FirstOrDefault();
            if (inventory != null)
            {
                inventory.AvailableQuantity += quantity;
                _inventory.ReplaceOne(i => i.Id == inventory.Id, inventory);
            }
        }

        // Reserve stock when an order is placed by a vendor
        public void ReserveStock(string productId, string vendorId, int quantity)
        {
            var inventory = _inventory.Find(i => i.ProductId == productId && i.VendorId == vendorId).FirstOrDefault();
            if (inventory != null && inventory.AvailableQuantity >= quantity)
            {
                inventory.AvailableQuantity -= quantity;
                inventory.ReservedQuantity += quantity;
                _inventory.ReplaceOne(i => i.Id == inventory.Id, inventory);
            }
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

        // Trigger low stock alert for a product by vendor
        public void CheckReorderLevel(string productId, string vendorId)
        {
            var inventory = _inventory.Find(i => i.ProductId == productId && i.VendorId == vendorId).FirstOrDefault();
            if (inventory != null && inventory.AvailableQuantity <= inventory.ReorderLevel)
            {
                // Notify the vendor (e.g., send an email or push notification) using vendorId
            }
        }
    }
}
