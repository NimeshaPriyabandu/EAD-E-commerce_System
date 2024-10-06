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

        public VendorController(VendorService vendorService)
        {
            _vendorService = vendorService;
        }

        [HttpPost("{vendorId}/addcomments")] // Only customers can leave comments
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

                // Set the CustomerId in the rating
                rating.CustomerId = customerId;

                // Log the current state of the rating object before passing it to the service
                Console.WriteLine($"Rating Object: Rating = {rating.Rating}, Comment = {rating.Comment}, CustomerId = {rating.CustomerId}");

                // Add the rating and comment to the vendor
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


        [HttpGet("{vendorId}/comments")]
        public async Task<IActionResult> GetVendorComments(string vendorId)
        {
            var comments = await _vendorService.GetVendorComments(vendorId);
            return Ok(comments);
        }


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
