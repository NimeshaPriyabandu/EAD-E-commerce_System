// -----------------------------------------------------------------------------
// ProductsController.cs
// 
// This controller handles operations related to managing products, including 
// creating, updating, and deleting products. It allows vendors to manage their 
// products, and users to browse products by category or vendor. The controller 
// also manages stock updates via the InventoryService.
// -----------------------------------------------------------------------------

using E_commerce_system.Models;
using E_commerce_system.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace E_commerce_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly InventoryService _inventoryService;

        // Constructor to initialize services.
        public ProductsController(ProductService productService, InventoryService inventoryService)
        {
            _productService = productService;
            _inventoryService = inventoryService;
        }

        // Get all products.
        [HttpGet]
        public ActionResult<List<Product>> GetAll()
        {
            var products = _productService.Get(); 
            return Ok(products);
        }

        // Get product by ID.
        [HttpGet("{id:length(24)}", Name = "GetProduct")]
        public ActionResult<Product> GetById(string id)
        {
            var product = _productService.Get(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        // Create a new product (vendor only).
        [HttpPost]
        public ActionResult<Product> Create(Product product)
        {
            var vendorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            product.VendorId = vendorId;

            if (string.IsNullOrEmpty(product.ImageUrl))
            {
                product.ImageUrl = "https://example.com/default-product-image.jpg";
            }

            _productService.Create(product);
            _inventoryService.UpdateStock(product.Id, vendorId, product.Stock);
            return CreatedAtRoute("GetProduct", new { id = product.Id }, product); 
        }

        // Update a product (vendor only).
        [HttpPut("{id:length(24)}")]
        [Authorize(Roles = "Vendor")]
        public IActionResult Update(string id, Product updatedProduct)
        {
            var product = _productService.Get(id);
            if (product == null)
            {
                return NotFound(); 
            }

            var vendorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (product.VendorId != vendorId)
            {
                return Forbid("You are not authorized to update this product."); 
            }

            updatedProduct.Id = product.Id; 
            updatedProduct.VendorId = vendorId; 
            _productService.Update(id, vendorId, updatedProduct);
            _inventoryService.UpdateStock(id, vendorId, updatedProduct.Stock);

            return NoContent(); 
        }

        // Delete a product (vendor only).
        [HttpDelete("{id:length(24)}")]
        [Authorize(Roles = "Vendor")]
        public IActionResult Delete(string id)
        {
            var product = _productService.Get(id);
            if (product == null)
            {
                return NotFound(); 
            }

            var vendorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (product.VendorId != vendorId)
            {
                return Forbid("You are not authorized to delete this product."); 
            }

            _productService.Remove(id, vendorId);
            _inventoryService.UpdateStock(id, vendorId, -product.Stock);

            return NoContent(); 
        }

        // Activate a product (vendor only).
        [HttpPut("{id:length(24)}/activate")]
        public IActionResult ActivateProduct(string id)
        {
            var product = _productService.Get(id);
            if (product == null)
            {
                return NotFound();
            }

            var vendorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _productService.ActivateProduct(id, vendorId);
            return NoContent();
        }

        // Deactivate a product (vendor only).
        [HttpPut("{id:length(24)}/deactivate")]
        public IActionResult DeactivateProduct(string id)
        {
            var product = _productService.Get(id);
            if (product == null)
            {
                return NotFound();
            }

            var vendorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _productService.DeactivateProduct(id, vendorId);
            return NoContent();
        }

        // Get products by category.
        [HttpGet("category/{category}")]
        public ActionResult<List<Product>> GetProductsByCategory(string category)
        {
            var products = _productService.GetByCategory(category);
            if (products.Count == 0)
            {
                return NotFound(new { message = "No products found in this category." });
            }
            return Ok(products);
        }

        // Get products by vendor (vendor only).
        [HttpGet("vendor")]
        public ActionResult<List<Product>> GetProductsByVendor()
        {
            var vendorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(vendorId))
            {
                return Unauthorized(new { message = "Vendor ID not found in token." });
            }

            var products = _productService.GetProductsByVendor(vendorId);

            if (products == null || products.Count == 0)
            {
                return NotFound(new { message = "No products found for this vendor." });
            }
            
            return Ok(products); 
        }
    }
}
