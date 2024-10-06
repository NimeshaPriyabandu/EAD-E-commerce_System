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

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/orders
        [HttpGet]
        public ActionResult<List<Order>> GetAllOrders()
        {
            var orders = _orderService.GetAll();
            return Ok(new { message = "Orders retrieved successfully", orders });
        }

        // GET: api/orders/{id}
        [HttpGet("{id:length(24)}", Name = "GetOrder")]
        public ActionResult<Order> GetOrderById(string id)
        {
            var order = _orderService.GetById(id);
            if (order == null)
            {
                return NotFound(new { message = "Order not found" }); // Return 404 if order not found
            }
            return Ok(new { message = "Order retrieved successfully", order });
        }

        // POST: api/orders
        [HttpPost]
        public ActionResult CreateOrder([FromBody] Order order)
        {
            // Extract Customer ID from JWT token
            var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                return Unauthorized(new { message = "Customer ID not found in token." });
            }

            // Assign the customer ID to the order
            order.CustomerId = customerId;

            // Create the order
            var message = _orderService.Create(order);
            return Ok(new { message = message });
        }

        // PUT: api/orders/{id}
        [HttpPut("{id:length(24)}")]
        public IActionResult UpdateOrder(string id, [FromBody] Order updatedOrder)
        {
            var message = _orderService.Update(id, updatedOrder);
            if (message == "Order not found.")
            {
                return NotFound(new { message = message }); // Return 404 if order not found
            }

            return Ok(new { message = message }); // Return 200 with the message
        }

        // DELETE: api/orders/{id}
        [HttpDelete("{id:length(24)}")]
        public IActionResult CancelOrder(string id)
        {
            var message = _orderService.CancelOrder(id);
            if (message == "Order not found.")
            {
                return NotFound(new { message = message }); // Return 404 if order not found
            }

            return Ok(new { message = message }); // Return 200 with the message after cancellation
        }

        // PUT: api/orders/{id}/status
        [HttpPut("{id:length(24)}/status")]
        public IActionResult UpdateOrderStatus(string id, [FromBody] string newStatus)
        {
            var message = _orderService.UpdateOrderStatus(id, newStatus);
            if (message == "Order not found.")
            {
                return NotFound(new { message = message }); // Return 404 if order not found
            }

            return Ok(new { message = message }); // Return 200 with the updated status
        }

        [HttpGet("history")]
        public IActionResult GetOrderHistory()
        {
         
            var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                return Unauthorized(new { message = "Customer ID not found in token." });
            }

            // Fetch the orders for the customer
            var orders = _orderService.GetOrdersByCustomerId(customerId);
            if (orders.Count == 0)
            {
                return NotFound(new { message = "No orders found for this customer." });
            }

            return Ok(new { message = "Orders retrieved successfully", orders });
        }

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

        // PUT: api/orders/{id}/cancel-process
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
    }
}
