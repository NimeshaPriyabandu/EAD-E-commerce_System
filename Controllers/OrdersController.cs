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
        public ActionResult<List<Order>> GetAll()
        {
            return _orderService.Get();
        }

        // GET: api/orders/{id}
        [HttpGet("{id:length(24)}", Name = "GetOrder")]
        public ActionResult<Order> GetById(string id)
        {
            var order = _orderService.Get(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        // POST: api/orders
        [HttpPost]
        public ActionResult<Order> Create(Order order)
        {
            // Optional: Validate order details here

            var newOrder = _orderService.Create(order);
            return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
        }

        // PUT: api/orders/{id}
        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Order updatedOrder)
        {
            var result = _orderService.Update(id, updatedOrder);
            if (!result)
            {
                return BadRequest("Order not found or cannot be updated at this stage.");
            }

            return NoContent();
        }

        // DELETE: api/orders/{id}
        [HttpDelete("{id:length(24)}")]
        public IActionResult Cancel(string id)
        {
            var result = _orderService.CancelOrder(id);
            if (!result)
            {
                return BadRequest("Order not found or cannot be cancelled at this stage.");
            }

            return NoContent();
        }

        // PUT: api/orders/{id}/status
        [HttpPut("{id:length(24)}/status")]
        public IActionResult UpdateOrderStatus(string id, [FromBody] string newStatus)
        {
            var allowedStatuses = new List<string> { "Processing", "Dispatched", "Delivered", "Cancelled" };

            if (!allowedStatuses.Contains(newStatus))
            {
                return BadRequest("Invalid status value.");
            }

            var result = _orderService.UpdateOrderStatus(id, newStatus);
            if (!result)
            {
                return BadRequest("Order not found or cannot be updated to this status.");
            }

            return NoContent();
        }
    }
}
