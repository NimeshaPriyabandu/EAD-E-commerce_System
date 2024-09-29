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

        // Create a new product
        public Product Create(Product product)
        {
            _products.InsertOne(product);
            return product;
        }

        // Update an existing product
        public void Update(string id, Product productIn) =>
            _products.ReplaceOne(product => product.Id == id, productIn);

        // Remove a product
        public void Remove(string id) => _products.DeleteOne(product => product.Id == id);

        // Activate a product
        public void ActivateProduct(string id) =>
            _products.UpdateOne(product => product.Id == id, Builders<Product>.Update.Set(p => p.IsActive, true));

        // Deactivate a product
        public void DeactivateProduct(string id) =>
            _products.UpdateOne(product => product.Id == id, Builders<Product>.Update.Set(p => p.IsActive, false));
    }
}
