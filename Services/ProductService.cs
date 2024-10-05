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

        public ProductService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _products = database.GetCollection<Product>("Products");
        }

        // Get all products
        public List<Product> Get() => _products.Find(product => true).ToList();

        // Get a product by ID
        public Product Get(string id) => _products.Find<Product>(product => product.Id == id).FirstOrDefault();

        public List<Product> GetProductsByVendor(string vendorId)
        {
            return _products.Find(product => product.VendorId == vendorId).ToList();
        }

        // Create a new product
        public Product Create(Product product)
        {
            _products.InsertOne(product);
            return product;
        }

        
        // public void Update(string id, Product productIn) =>
        //     _products.ReplaceOne(product => product.Id == id, productIn);

        // // Remove a product
        // public void Remove(string id) => _products.DeleteOne(product => product.Id == id);

        // // Activate a product
        // public void ActivateProduct(string id) =>
        //     _products.UpdateOne(product => product.Id == id, Builders<Product>.Update.Set(p => p.IsActive, true));

        // // Deactivate a product
        // public void DeactivateProduct(string id) =>
        //     _products.UpdateOne(product => product.Id == id, Builders<Product>.Update.Set(p => p.IsActive, false));

        public bool Update(string id, string vendorId, Product productIn)
        {
            var product = Get(id);

            // Check if the vendor owns the product
            if (product == null || product.VendorId != vendorId)
            {
                return false; // Return false if vendor doesn't own the product or product doesn't exist
            }

            _products.ReplaceOne(product => product.Id == id, productIn);
            return true; // Return true on successful update
        }

        public bool Remove(string id, string vendorId)
        {
            var product = Get(id);

            // Check if the vendor owns the product
            if (product == null || product.VendorId != vendorId)
            {
                return false; // Return false if vendor doesn't own the product or product doesn't exist
            }

            _products.DeleteOne(product => product.Id == id);
            return true; // Return true on successful deletion
        }

        public bool ActivateProduct(string id, string vendorId)
        {
            var product = Get(id);

            // Check if the vendor owns the product
            if (product == null || product.VendorId != vendorId)
            {
                return false; // Return false if vendor doesn't own the product or product doesn't exist
            }

            _products.UpdateOne(product => product.Id == id, Builders<Product>.Update.Set(p => p.IsActive, true));
            return true; // Return true on successful activation
        }

        // Deactivate a product, ensure vendor owns the product
        public bool DeactivateProduct(string id, string vendorId)
        {
            var product = Get(id);

            // Check if the vendor owns the product
            if (product == null || product.VendorId != vendorId)
            {
                return false; // Return false if vendor doesn't own the product or product doesn't exist
            }

            _products.UpdateOne(product => product.Id == id, Builders<Product>.Update.Set(p => p.IsActive, false));
            return true; // Return true on successful deactivation
        }

        public List<Product> GetByCategory(string category)
        {
            return _products.Find(product => product.Category == category).ToList();
        }
    }
}
