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
            return Ok(_orderService.GetAll());
        }

        // GET: api/orders/{id}
        [HttpGet("{id:length(24)}", Name = "GetOrder")]
        public ActionResult<Order> GetOrderById(string id)
        {
            var order = _orderService.GetById(id);
            if (order == null)
            {
                return NotFound(); // Return 404 if order not found
            }
            return Ok(order);
        }

        // POST: api/orders
        [HttpPost]
        public ActionResult<Order> CreateOrder([FromBody] Order order)
        {
            var createdOrder = _orderService.Create(order);
            return CreatedAtRoute("GetOrder", new { id = createdOrder.Id }, createdOrder);
        }

        // PUT: api/orders/{id}
        [HttpPut("{id:length(24)}")]
        public IActionResult UpdateOrder(string id, [FromBody] Order updatedOrder)
        {
            var existingOrder = _orderService.GetById(id);
            if (existingOrder == null)
            {
                return NotFound(); // Return 404 if order not found
            }

            updatedOrder.Id = id;
            _orderService.Update(id, updatedOrder);
            return NoContent(); // Return 204 for a successful update
        }

        // DELETE: api/orders/{id}
        [HttpDelete("{id:length(24)}")]
        public IActionResult CancelOrder(string id)
        {
            var existingOrder = _orderService.GetById(id);
            if (existingOrder == null)
            {
                return NotFound(); // Return 404 if order not found
            }

            _orderService.CancelOrder(id);
            return NoContent(); // Return 204 after cancelling the order
        }

        // PUT: api/orders/{id}/status
        [HttpPut("{id:length(24)}/status")]
        public IActionResult UpdateOrderStatus(string id, [FromBody] string newStatus)
        {
            var existingOrder = _orderService.GetById(id);
            if (existingOrder == null)
            {
                return NotFound(); // Return 404 if order not found
            }

            _orderService.UpdateOrderStatus(id, newStatus);
            return NoContent(); // Return 204 after updating the order status
        }
    }
}
