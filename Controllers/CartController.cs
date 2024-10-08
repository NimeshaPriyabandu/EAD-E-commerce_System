// -----------------------------------------------------------------------------
// CartController.cs
// 
// This controller handles operations related to the shopping cart, including 
// adding, updating, and removing items in the cart. It also allows users to 
// clear their cart and retrieve the current state of their cart.
// -----------------------------------------------------------------------------

using E_commerce_system.Models;
using E_commerce_system.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace E_commerce_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;
        private readonly UserService _userService;

        // Constructor to initialize CartService and UserService.
        public CartController(CartService cartService, UserService userService)
        {
            _cartService = cartService;
            _userService = userService;
        }

        // Get the cart for the logged-in user.
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null) return NotFound(new { message = "Cart not found." });

            return Ok(cart);
        }

        // Add a product to the cart.
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var success = await _cartService.AddToCartAsync(userId, request.ProductId, request.Quantity);
            if (!success) return NotFound(new { message = "Product not found." });

            return Ok(new { message = "Product added to cart." });
        }

        // Update the quantity of an item in the cart.
        [HttpPost("update")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var success = await _cartService.UpdateCartItemQuantityAsync(userId, request.ProductId, request.Quantity);
            if (!success) return NotFound(new { message = "Product not found in cart." });

            return Ok(new { message = "Cart item updated." });
        }

        // Remove a product from the cart.
        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveFromCart(string productId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var success = await _cartService.RemoveFromCartAsync(userId, productId);
            if (!success) return NotFound(new { message = "Product not found in cart." });

            return Ok(new { message = "Product removed from cart." });
        }

        // Clear the entire cart.
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var success = await _cartService.ClearCartAsync(userId);
            if (!success) return NotFound(new { message = "Cart not found." });

            return Ok(new { message = "Cart cleared." });
        }
    }

    // DTO for adding a product to the cart.
    public class AddToCartRequest
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }

    // DTO for updating the quantity of a cart item.
    public class UpdateCartItemRequest
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
