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
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService _inventoryService;

        public InventoryController(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // Check stock for a product by a specific vendor
        [HttpGet("checkStock/{productId}/{vendorId}/{quantity}")]
        public ActionResult<bool> CheckStock(string productId, string vendorId, int quantity)
        {
            var isStockAvailable = _inventoryService.CheckStock(productId, vendorId, quantity);
            if (!isStockAvailable)
            {
                return NotFound("Insufficient stock");
            }
            return Ok(true); // Stock is available
        }

        // Reserve stock for a product by a vendor
        [HttpPost("reserveStock/{productId}/{vendorId}/{quantity}")]
        public IActionResult ReserveStock(string productId, string vendorId, int quantity)
        {
            _inventoryService.ReserveStock(productId, vendorId, quantity);
            return Ok("Stock reserved");
        }

        // Release stock for a product by a vendor
        [HttpPost("releaseStock/{productId}/{vendorId}/{quantity}")]
        public IActionResult ReleaseStock(string productId, string vendorId, int quantity)
        {
            _inventoryService.ReleaseStock(productId, vendorId, quantity);
            return Ok("Stock released");
        }

        // Update stock for a product by a vendor
        [HttpPut("updateStock/{productId}/{vendorId}/{quantity}")]
        public IActionResult UpdateStock(string productId, string vendorId, int quantity)
        {
            _inventoryService.UpdateStock(productId, vendorId, quantity);
            return Ok("Stock updated");
        }

        // Check reorder level and notify vendor
        [HttpGet("checkReorderLevel/{productId}/{vendorId}")]
        public IActionResult CheckReorderLevel(string productId, string vendorId)
        {
            _inventoryService.CheckReorderLevel(productId, vendorId);
            return Ok("Reorder level checked");
        }

        // Get all notifications for a specific vendor
        [HttpGet("notifications/{vendorId}")]
        public IActionResult GetNotifications(string vendorId)
        {
            // Fetch all inventories for the given vendor
            var notifications = _inventoryService.GetVendorNotifications(vendorId);

            // Check if there are any notifications
            if (notifications.Count == 0)
            {
                return NotFound(new { message = "No notifications found for this vendor." });
            }

            return Ok(new { message = "Notifications retrieved successfully.", notifications });
        }

        [HttpGet("vendor/stocks")]
        public IActionResult GetStocksByVendor()
        {
            // Retrieve vendor ID from JWT token
            var vendorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(vendorId))
            {
                return Unauthorized(new { message = "Vendor ID not found in token." });
            }

            // Fetch all inventory records for the vendor
            var vendorStocks = _inventoryService.GetStocksByVendor(vendorId);

            // Check if the vendor has any stock
            if (vendorStocks.Count == 0)
            {
                return NotFound(new { message = "No stock records found for this vendor." });
            }

            return Ok(new { message = "Stock records retrieved successfully.", vendorStocks });
        }


    }
}
