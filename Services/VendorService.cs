using E_commerce_system.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace E_commerce_system.Services
{
    public class VendorService
    {
        private readonly IMongoCollection<User> _users;

        public VendorService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("Users"); // Single collection for both users and vendors
        }

        // Get a vendor by ID
        public async Task<Vendor?> GetByIdAsync(string vendorId)
        {
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, vendorId),
                Builders<User>.Filter.Eq(u => u.Role, "Vendor")
            );

            return await _users.Find(filter).FirstOrDefaultAsync() as Vendor; // Cast to Vendor
        }

        // Update vendor details (including ratings and comments)
        public async Task UpdateVendorAsync(Vendor vendor)
        {
            await _users.ReplaceOneAsync(u => u.Id == vendor.Id, vendor);
        }

        // Add rating and comment to vendor
        public async Task AddRatingAndComment(string vendorId, CustomerRating rating)
        {
            var vendor = await GetByIdAsync(vendorId);
            if (vendor == null)
            {
                throw new Exception("Vendor not found");
            }

            // Fetch customer details using the CustomerId
            var customer = await GetCustomerDetails(rating.CustomerId);
            if (customer == null)
            {
                throw new Exception("Customer not found");
            }

            // Set the Customer field
            rating.Customer = customer;

            vendor.Ratings.Add(rating);
            vendor.AverageRating = vendor.Ratings.Average(r => r.Rating);

            await UpdateVendorAsync(vendor);
        }



        // Get all comments for a vendor, now including customer details
        public async Task<List<CustomerRating>> GetVendorComments(string vendorId)
        {
            var vendor = await GetByIdAsync(vendorId);
            if (vendor == null)
            {
                return new List<CustomerRating>();
            }

            // Fetch customer details for each rating
            foreach (var rating in vendor.Ratings)
            {
                rating.Customer = await GetCustomerDetails(rating.CustomerId);
            }

            return vendor.Ratings;
        }

        public async Task<List<Vendor>> GetAllVendorsAsync()
        {
            var filter = Builders<User>.Filter.Eq(u => u.Role, "Vendor");
            var users = await _users.Find(filter).ToListAsync();

            return users.OfType<Vendor>().ToList(); // Cast Users to Vendors
        }

        public async Task<List<CustomerRating>> GetRatingsByCustomer(string customerId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Role, "Vendor");
            var vendors = await _users.Find(filter).ToListAsync();

            var customerRatings = new List<CustomerRating>();
            foreach (var vendor in vendors.OfType<Vendor>())
            {
                foreach (var rating in vendor.Ratings.Where(rating => rating.CustomerId == customerId))
                {
                    rating.Customer = await GetCustomerDetails(rating.CustomerId);
                    customerRatings.Add(rating);
                }
            }

            return customerRatings;
        }

        
        private async Task<User?> GetCustomerDetails(string customerId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, customerId);
            return await _users.Find(filter).FirstOrDefaultAsync(); // Fetch the customer (User) by Id
        }

        public async Task<List<CustomerRating>> GetAllRatingsAndCommentsForVendorAsync(string vendorId)
        {
            var vendor = await GetByIdAsync(vendorId);
            if (vendor == null)
            {
                throw new Exception("Vendor not found");
            }

            // Return the list of customer ratings
            return vendor.Ratings;
        }
    }
}
