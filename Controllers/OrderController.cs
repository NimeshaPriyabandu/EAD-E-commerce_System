// -----------------------------------------------------------------------------
// OrdersController.cs
// 
// This controller handles operations related to order management, including 
// creating, updating, and canceling orders. It also handles stock reservations, 
// retrieving order history for customers, and managing vendor order deliveries.
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
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly InventoryService _inventoryService;

        // Constructor to initialize services.
        public OrdersController(OrderService orderService, InventoryService inventoryService)
        {
            _orderService = orderService;
            _inventoryService = inventoryService;
        }

        // Get all orders.
        [HttpGet]
        public ActionResult<List<Order>> GetAllOrders()
        {
            var orders = _orderService.GetAll();
            return Ok(new { message = "Orders retrieved successfully", orders });
        }

        // Get a specific order by ID.
        [HttpGet("{id:length(24)}", Name = "GetOrder")]
        public ActionResult<Order> GetOrderById(string id)
        {
            var order = _orderService.GetById(id);
            if (order == null)
            {
                return NotFound(new { message = "Order not found" }); 
            }
            return Ok(new { message = "Order retrieved successfully", order });
        }

        // Create a new order.
        [HttpPost]
        public ActionResult CreateOrder([FromBody] Order order)
        {
            var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                return Unauthorized(new { message = "Customer ID not found in token." });
            }

            order.CustomerId = customerId;

            foreach (var orderItem in order.Items)
            {
                bool stockAvailable = _inventoryService.CheckStock(orderItem.ProductId, orderItem.VendorId, orderItem.Quantity);
                if (!stockAvailable)
                {
                    return BadRequest(new { message = $"Insufficient stock for product {orderItem.ProductName}. Please reduce quantity or try again later." });
                }

                bool stockReserved = _inventoryService.ReserveStock(orderItem.ProductId, orderItem.VendorId, orderItem.Quantity);
                if (!stockReserved)
                {
                    return BadRequest(new { message = $"Unable to reserve stock for product {orderItem.ProductId}. Please try again later." });
                }

                _inventoryService.CheckReorderLevel(orderItem.ProductId, orderItem.VendorId);
            }

            var message = _orderService.Create(order);
            return Ok(new { message = message });
        }

        // Update an existing order.
        [HttpPut("{id:length(24)}")]
        public IActionResult UpdateOrder(string id, [FromBody] Order updatedOrder)
        {
            var message = _orderService.Update(id, updatedOrder);
            if (message == "Order not found.")
            {
                return NotFound(new { message = message }); 
            }

            return Ok(new { message = message });
        }

        // Cancel an order.
        [HttpDelete("{id:length(24)}")]
        public IActionResult CancelOrder(string id)
        {
            var message = _orderService.CancelOrder(id);
            if (message == "Order not found.")
            {
                return NotFound(new { message = message }); 
            }

            return Ok(new { message = message }); 
        }

        // Update the status of an order.
        [HttpPut("{id:length(24)}/status")]
        public IActionResult UpdateOrderStatus(string id, [FromBody] string newStatus)
        {
            var message = _orderService.UpdateOrderStatus(id, newStatus);
            if (message == "Order not found.")
            {
                return NotFound(new { message = message }); 
            }

            return Ok(new { message = message }); 
        }

        // Get order history for the logged-in customer.
        [HttpGet("history")]
        public IActionResult GetOrderHistory()
        {
            var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                return Unauthorized(new { message = "Customer ID not found in token." });
            }

            var orders = _orderService.GetOrdersByCustomerId(customerId);
            if (orders.Count == 0)
            {
                return NotFound(new { message = "No orders found for this customer." });
            }

            return Ok(new { message = "Orders retrieved successfully", orders });
        }

        // Request cancellation of an order.
        [HttpPost("{id}/cancel-request")]
        public IActionResult RequestCancellation(string id)
        {
            var message = _orderService.RequestOrderCancellation(id);
            if (message == "Order not found.")
            {
                return NotFound(new { message = message });
            }

            return Ok(new { message = message });
        }

        // Process a cancellation request (Approve/Reject).
        [HttpPut("{id}/cancel-process")]
        public IActionResult ProcessCancellation(string id, [FromBody] string action)
        {
            var message = _orderService.ProcessCancellationRequest(id, action);
            if (message == "Order not found.")
            {
                return NotFound(new { message = message });
            }

            return Ok(new { message = message });
        }

        // Get all cancellation requests.
        [HttpGet("cancellation-requests")]
        public IActionResult GetAllCancellationRequests()
        {
            var cancellationRequests = _orderService.GetAllCancellationRequests();
            if (cancellationRequests.Count == 0)
            {
                return NotFound(new { message = "No cancellation requests found." });
            }
            return Ok(new { message = "Cancellation requests retrieved successfully.", cancellationRequests });
        }

        // Mark an item as delivered by a vendor.
        [HttpPut("{orderId}/vendor/deliver/{productId}")]
        [Authorize(Roles = "Vendor")]
        public IActionResult MarkItemAsDelivered(string orderId, string productId)
        {
            var vendorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(vendorId))
            {
                return Unauthorized(new { message = "Vendor ID not found in token." });
            }

            var message = _orderService.MarkItemAsDelivered(orderId, vendorId, productId);

            if (message == "Order not found.")
            {
                return NotFound(new { message });
            }

            if (message == "Order item not found for this vendor and product.")
            {
                return NotFound(new { message });
            }

            return Ok(new { message });
        }

        // Get all orders associated with the logged-in vendor.
        [HttpGet("vendor/orders")]
        [Authorize(Roles = "Vendor")]
        public IActionResult GetOrdersByVendor()
        {
            var vendorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(vendorId))
            {
                return Unauthorized(new { message = "Vendor ID not found in token." });
            }

            var orders = _orderService.GetOrdersByVendorId(vendorId);
            if (orders.Count == 0)
            {
                return NotFound(new { message = "No orders found for this vendor." });
            }

            return Ok(new { message = "Orders retrieved successfully.", orders });
        }
    }
}
