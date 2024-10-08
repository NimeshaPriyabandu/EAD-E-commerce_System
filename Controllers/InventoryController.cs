// -----------------------------------------------------------------------------
// InventoryController.cs
// 
// This controller handles operations related to managing inventory, including 
// checking stock, reserving and releasing stock, updating stock levels, and 
// retrieving vendor-specific stock information. It also includes functionality 
// for checking reorder levels and retrieving notifications for low stock.
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
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService _inventoryService;

        // Constructor to initialize InventoryService.
        public InventoryController(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // Check if stock is available for a product and vendor.
        [HttpGet("checkStock/{productId}/{vendorId}/{quantity}")]
        public ActionResult<bool> CheckStock(string productId, string vendorId, int quantity)
        {
            var isStockAvailable = _inventoryService.CheckStock(productId, vendorId, quantity);
            if (!isStockAvailable)
            {
                return NotFound("Insufficient stock");
            }
            return Ok(true); 
        }

        // Reserve stock for a product and vendor.
        [HttpPost("reserveStock/{productId}/{vendorId}/{quantity}")]
        public IActionResult ReserveStock(string productId, string vendorId, int quantity)
        {
            _inventoryService.ReserveStock(productId, vendorId, quantity);
            return Ok("Stock reserved");
        }

        // Release reserved stock for a product and vendor.
        [HttpPost("releaseStock/{productId}/{vendorId}/{quantity}")]
        public IActionResult ReleaseStock(string productId, string vendorId, int quantity)
        {
            _inventoryService.ReleaseStock(productId, vendorId, quantity);
            return Ok("Stock released");
        }

        // Update stock quantity for a product and vendor.
        [HttpPut("updateStock/{productId}/{vendorId}/{quantity}")]
        public IActionResult UpdateStock(string productId, string vendorId, int quantity)
        {
            _inventoryService.UpdateStock(productId, vendorId, quantity);
            return Ok("Stock updated");
        }

        // Check the reorder level for a product and vendor.
        [HttpGet("checkReorderLevel/{productId}/{vendorId}")]
        public IActionResult CheckReorderLevel(string productId, string vendorId)
        {
            _inventoryService.CheckReorderLevel(productId, vendorId);
            return Ok("Reorder level checked");
        }

        // Get notifications for a vendor regarding low stock.
        [HttpGet("notifications/{vendorId}")]
        public IActionResult GetNotifications(string vendorId)
        {
            var notifications = _inventoryService.GetVendorNotifications(vendorId);

            if (notifications.Count == 0)
            {
                return NotFound(new { message = "No notifications found for this vendor." });
            }

            return Ok(new { message = "Notifications retrieved successfully.", notifications });
        }

        // Get all stock records for the logged-in vendor.
        [HttpGet("vendor/stocks")]
        public IActionResult GetStocksByVendor()
        {
            var vendorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(vendorId))
            {
                return Unauthorized(new { message = "Vendor ID not found in token." });
            }

            var vendorStocks = _inventoryService.GetStocksByVendor(vendorId);

            if (vendorStocks.Count == 0)
            {
                return NotFound(new { message = "No stock records found for this vendor." });
            }

            return Ok(new { message = "Stock records retrieved successfully.", vendorStocks });
        }
    }
}
