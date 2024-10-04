using E_commerce_system.Models;
using E_commerce_system.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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
            var message = _orderService.Create(order);
            return Ok(new { message = message }); // Explicitly returning the message in JSON format
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
    }
}
