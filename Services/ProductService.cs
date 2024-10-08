// -----------------------------------------------------------------------------
// ProductService.cs
// 
// This service class provides operations related to product management, 
// including creating, updating, deleting, activating, deactivating, and 
// retrieving products. It also allows filtering products by vendor and category.
// -----------------------------------------------------------------------------

using E_commerce_system.Configurations;
using E_commerce_system.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;

namespace E_commerce_system.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _products;

        // Constructor to initialize MongoDB connection and collection.
        public ProductService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _products = database.GetCollection<Product>("Products");
        }

        // Get all products.
        public List<Product> Get() => _products.Find(product => true).ToList();

        // Get product by ID.
        public Product Get(string id) => _products.Find<Product>(product => product.Id == id).FirstOrDefault();

        // Get products by vendor ID.
        public List<Product> GetProductsByVendor(string vendorId)
        {
            return _products.Find(product => product.VendorId == vendorId).ToList();
        }

        // Create a new product.
        public Product Create(Product product)
        {
            _products.InsertOne(product);
            return product;
        }

        // Update a product, ensure vendor owns the product.
        public bool Update(string id, string vendorId, Product productIn)
        {
            var product = Get(id);

            // Check if the vendor owns the product.
            if (product == null || product.VendorId != vendorId)
            {
                return false; // Vendor does not own product or product not found.
            }

            _products.ReplaceOne(product => product.Id == id, productIn);
            return true; // Update successful.
        }

        // Remove a product, ensure vendor owns the product.
        public bool Remove(string id, string vendorId)
        {
            var product = Get(id);

            // Check if the vendor owns the product.
            if (product == null || product.VendorId != vendorId)
            {
                return false; // Vendor does not own product or product not found.
            }

            _products.DeleteOne(product => product.Id == id);
            return true; // Deletion successful.
        }

        // Activate a product, ensure vendor owns the product.
        public bool ActivateProduct(string id, string vendorId)
        {
            var product = Get(id);

            // Check if the vendor owns the product.
            if (product == null || product.VendorId != vendorId)
            {
                return false; // Vendor does not own product or product not found.
            }

            _products.UpdateOne(product => product.Id == id, Builders<Product>.Update.Set(p => p.IsActive, true));
            return true; // Activation successful.
        }

        // Deactivate a product, ensure vendor owns the product.
        public bool DeactivateProduct(string id, string vendorId)
        {
            var product = Get(id);

            // Check if the vendor owns the product.
            if (product == null || product.VendorId != vendorId)
            {
                return false; // Vendor does not own product or product not found.
            }

            _products.UpdateOne(product => product.Id == id, Builders<Product>.Update.Set(p => p.IsActive, false));
            return true; // Deactivation successful.
        }

        // Get products by category.
        public List<Product> GetByCategory(string category)
        {
            return _products.Find(product => product.Category == category).ToList();
        }
    }
}
