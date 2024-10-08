// -----------------------------------------------------------------------------
// VendorController.cs
// 
// This controller handles operations related to vendors, including adding 
// customer ratings and comments, retrieving vendor comments, and retrieving 
// vendor ratings. Customers can also view their own ratings and comments 
// associated with vendors.
// -----------------------------------------------------------------------------

using E_commerce_system.Models;
using E_commerce_system.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_commerce_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController : ControllerBase
    {
        private readonly VendorService _vendorService;

        // Constructor to initialize VendorService.
        public VendorController(VendorService vendorService)
        {
            _vendorService = vendorService;
        }

        // Add a customer rating and comment to a vendor.
        [HttpPost("{vendorId}/addcomments")] 
        public async Task<IActionResult> AddRating(string vendorId, [FromBody] CustomerRating rating)
        {
            try
            {
                Console.WriteLine("Starting AddRating method.");

                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(customerId))
                {
                    Console.WriteLine("Customer ID not found in token.");
                    return Unauthorized(new { message = "Customer ID not found in token." });
                }

                Console.WriteLine($"Customer ID extracted from token: {customerId}");

                rating.CustomerId = customerId;

                Console.WriteLine($"Rating Object: Rating = {rating.Rating}, Comment = {rating.Comment}, CustomerId = {rating.CustomerId}");

                await _vendorService.AddRatingAndComment(vendorId, rating);

                Console.WriteLine("Rating and comment added successfully.");
                return Ok(new { message = "Rating and comment added successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddRating method: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while adding the comment.", error = ex.Message });
            }
        }

        // Get comments for a vendor.
        [HttpGet("{vendorId}/comments")]
        public async Task<IActionResult> GetVendorComments(string vendorId)
        {
            var comments = await _vendorService.GetVendorComments(vendorId);
            return Ok(comments);
        }

        // Get all vendors.
        [HttpGet("vendors/")]
        public async Task<IActionResult> GetAllVendors()
        {
            try
            {
                Console.WriteLine("Attempting to retrieve all vendors...");

                var vendors = await _vendorService.GetAllVendorsAsync();
                
                if (vendors == null || vendors.Count == 0)
                {
                    Console.WriteLine("No vendors found.");
                    return NotFound(new { message = "No vendors found." });
                }

                Console.WriteLine($"Retrieved {vendors.Count} vendors.");
                return Ok(vendors);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving vendors: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving vendors.", error = ex.Message });
            }
        }

        // Get the average rating for a vendor.
        [HttpGet("{vendorId}/rating")]
        public async Task<IActionResult> GetVendorAverageRating(string vendorId)
        {
            var vendor = await _vendorService.GetByIdAsync(vendorId);
            if (vendor == null)
            {
                return NotFound(new { message = "Vendor not found." });
            }
            return Ok(new { AverageRating = vendor.AverageRating });
        }

        // Get all ratings and comments made by the logged-in customer.
        [HttpGet("my-ratings")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetCustomerRatingsAndComments()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId))
            {
                return Unauthorized(new { message = "Customer ID not found in token." });
            }

            var ratings = await _vendorService.GetRatingsByCustomer(customerId);
            return Ok(ratings);
        }
    }
}
