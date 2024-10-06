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

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        // GET: api/products
        [HttpGet]
        
        public ActionResult<List<Product>> GetAll()
        {
            var products = _productService.Get(); // Get the list of products
            return Ok(products);
            // Return 200 OK with the list of products
        }

        // GET: api/products/{id}
        [HttpGet("{id:length(24)}", Name = "GetProduct")]
        public ActionResult<Product> GetById(string id)
        {
            var product = _productService.Get(id);
            if (product == null)
            {
                return NotFound(); // Return 404 Not Found if the product doesn't exist
            }
            return Ok(product); // Return 200 OK with the product
        }

        // POST: api/products
        [HttpPost]
        public ActionResult<Product> Create(Product product)
        {
            // Get vendor ID from the JWT token
            var vendorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine(vendorId);

            // Assign the vendorId to the product
            product.VendorId = vendorId;

            if (string.IsNullOrEmpty(product.ImageUrl))
            {
                // Optional: Set a default image URL if none is provided
                product.ImageUrl = "https://example.com/default-product-image.jpg";
            }

            _productService.Create(product);
            return CreatedAtRoute("GetProduct", new { id = product.Id }, product); 
        }


        [HttpPut("{id:length(24)}")]
        [Authorize(Roles = "Vendor")]
        public IActionResult Update(string id, Product updatedProduct)
        {
            var product = _productService.Get(id);
            if (product == null)
            {
                return NotFound(); // Return 404 Not Found if the product doesn't exist
            }

            // Get vendor ID from the JWT token
            var vendorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check if the logged-in vendor owns the product
            if (product.VendorId != vendorId)
            {
                return Forbid("You are not authorized to update this product."); // Return 403 Forbidden
            }

            updatedProduct.Id = product.Id; // Ensure the ID remains unchanged
            updatedProduct.VendorId = vendorId; // Ensure the VendorId remains unchanged
            _productService.Update(id, vendorId,updatedProduct);

            return NoContent(); // Return 204 No Content on successful update
        }

        // DELETE: api/products/{id} (Only vendors can delete their own products)
        [HttpDelete("{id:length(24)}")]
        [Authorize(Roles = "Vendor")]
        public IActionResult Delete(string id)
        {
            var product = _productService.Get(id);
            if (product == null)
            {
                return NotFound(); // Return 404 Not Found if the product doesn't exist
            }

            // Get vendor ID from the JWT token
            var vendorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (product.VendorId != vendorId)
            {
                return Forbid("You are not authorized to delete this product."); // Return 403 Forbidden
            }

            _productService.Remove(id,vendorId);
            return NoContent(); // Return 204 No Content on successful deletion
        }

        // PUT: api/products/{id}/activate (Only vendors can activate their own products)
        [HttpPut("{id:length(24)}/activate")]
        [Authorize(Roles = "Vendor")]
        public IActionResult ActivateProduct(string id)
        {
            var product = _productService.Get(id);
            if (product == null)
            {
                return NotFound();
            }

            var vendorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (product.VendorId != vendorId || userRole != "Administrator")
            {
                return Forbid("You are not authorized to activate this product."); // Return 403 Forbidden
            }

            _productService.ActivateProduct(id, vendorId);
            return NoContent();
        }

        // PUT: api/products/{id}/deactivate (Only vendors can deactivate their own products)
        [HttpPut("{id:length(24)}/deactivate")]
        [Authorize(Roles = "Vendor")]
        public IActionResult DeactivateProduct(string id)
        {
            var product = _productService.Get(id);
            if (product == null)
            {
                return NotFound();
            }

            // Get vendor ID from the JWT token
            var vendorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Check if the logged-in vendor owns the product or if the user is an Administrator
            if (product.VendorId != vendorId || userRole != "Administrator")
            {
                return Forbid("You are not authorized to activate this product."); // Return 403 Forbidden
            }

            _productService.DeactivateProduct(id, vendorId);
            return NoContent();
        }

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


        [HttpGet("vendor")]
        public ActionResult<List<Product>> GetProductsByVendor()
        {
            // Get vendor ID from the JWT token
            var vendorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // If vendorId is null, return unauthorized
            if (string.IsNullOrEmpty(vendorId))
            {
                return Unauthorized(new { message = "Vendor ID not found in token." });
            }

            // Call the ProductService to get products by vendor
            var products = _productService.GetProductsByVendor(vendorId);

            if (products == null || products.Count == 0)
            {
                return NotFound(new { message = "No products found for this vendor." });
            }
            
            return Ok(products); // Return 200 OK with the products list
        }

    }
}
