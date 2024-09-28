using E_commerce_system.Models;
using E_commerce_system.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace E_commerce_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        // Constructor Injection of the ProductService
        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        // GET: api/products
        [HttpGet]
        public ActionResult<List<Product>> GetAll()
        {
            return _productService.Get(); // Return 200 OK with the list of products
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
            _productService.Create(product);
            return CreatedAtRoute("GetProduct", new { id = product.Id }, product); // Return 201 Created with the product URI
        }

        // PUT: api/products/{id}
        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Product updatedProduct)
        {
            var product = _productService.Get(id);
            if (product == null)
            {
                return NotFound(); // Return 404 Not Found if the product doesn't exist
            }

            updatedProduct.Id = product.Id;
            _productService.Update(id, updatedProduct);

            return NoContent(); // Return 204 No Content on successful update
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var product = _productService.Get(id);
            if (product == null)
            {
                return NotFound(); // Return 404 Not Found if the product doesn't exist
            }

            _productService.Remove(id);
            return NoContent(); // Return 204 No Content on successful deletion
        }
    }
}
