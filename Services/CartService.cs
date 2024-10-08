// -----------------------------------------------------------------------------
// CartService.cs
// 
// Provides services for cart management, including adding, updating, 
// and removing items in the user's cart, as well as calculating the total price.
// -----------------------------------------------------------------------------

using E_commerce_system.Models;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;

namespace E_commerce_system.Services
{
    public class CartService
    {
        private readonly IMongoCollection<Cart> _carts;
        private readonly IMongoCollection<Product> _products;

        // Constructor to initialize collections.
        public CartService(IMongoDatabase database)
        {
            _carts = database.GetCollection<Cart>("Carts");
            _products = database.GetCollection<Product>("Products");
        }

        // Get the cart by userId.
        public async Task<Cart> GetCartByUserIdAsync(string userId)
        {
            return await _carts.Find(c => c.UserId == userId).FirstOrDefaultAsync();
        }

        // Add product to cart or update quantity.
        public async Task<bool> AddToCartAsync(string userId, string productId, int quantity)
        {
            var product = await _products.Find(p => p.Id == productId).FirstOrDefaultAsync();
            if (product == null) return false;

            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _carts.InsertOneAsync(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    Quantity = quantity,
                    Price = product.Price
                });
            }

            cart.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            await _carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);
            return true;
        }

        // Remove product from cart.
        public async Task<bool> RemoveFromCartAsync(string userId, string productId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null) return false;

            cart.Items.RemoveAll(i => i.ProductId == productId);
            cart.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            await _carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);
            return true;
        }

        // Update quantity of an item in the cart.
        public async Task<bool> UpdateCartItemQuantityAsync(string userId, string productId, int quantity)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null) return false;

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null) return false;

            item.Quantity = quantity;
            cart.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            await _carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);
            return true;
        }

        // Clear all items in the cart.
        public async Task<bool> ClearCartAsync(string userId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null) return false;

            cart.Items.Clear();
            cart.TotalPrice = 0;
            await _carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);
            return true;
        }
    }
}
