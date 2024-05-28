using Backend.Dtos; // Importing DTOs namespace for data transfer objects
using Backend.Models; // Importing models namespace for Product model
using Microsoft.EntityFrameworkCore; // Importing Entity Framework Core namespace
using Backend.Helpers; // Importing helpers namespace for utility functions

namespace Backend.Services // Defining namespace for services
{
    public class ProductService // ProductService class for handling product-related operations
    {
        private readonly EcommerceSdaContext _dbContext; // Database context for interacting with database

        public ProductService(EcommerceSdaContext dbContext) // Constructor for injecting database context dependency
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext)); // Null check for dbContext
        }

        // Method for retrieving all products with pagination and filtering
        public async Task<PaginationResult<Product>> GetAllProductsAsync(
            string? productName = null, // Optional parameter for filtering by product name
            decimal? minPrice = null, // Optional parameter for filtering by minimum price
            decimal? maxPrice = null, // Optional parameter for filtering by maximum price
            DateTime? createDate = null, // Optional parameter for filtering by creation date
            string sortBy = "id", // Optional parameter for sorting
            bool ascending = true, // Optional parameter for sorting order
            int pageNumber = 1, // Optional parameter for pagination - page number
            int pageSize = 100,
            int? categoryId = null // Add categoryId parameter
            ) // Optional parameter for pagination - page size
        {
            try
            {
                var query = _dbContext.Products.AsQueryable(); // Creating queryable object for products

                // Apply filters based on provided parameters
                if (!string.IsNullOrEmpty(productName))
                {
                    var productNameLower = productName.ToLower();
                    query = query.Where(p => p.ProductName.ToLower().Contains(productNameLower));
                }

                if (minPrice.HasValue)
                    query = query.Where(p => p.ProductPrice >= minPrice);

                if (maxPrice.HasValue)
                    query = query.Where(p => p.ProductPrice <= maxPrice);

                if (createDate.HasValue)
                    query = query.Where(p => p.CreatedAt >= createDate);

                if (categoryId.HasValue)
                    query = query.Where(p => p.CategoryId == categoryId);

                // Apply sorting based on provided sortBy parameter
                query = sortBy.ToLower() switch
                {
                    "price" => ascending ? query.OrderBy(p => p.ProductPrice) : query.OrderByDescending(p => p.ProductPrice),
                    "product name" => ascending ? query.OrderBy(p => p.ProductName) : query.OrderByDescending(p => p.ProductName),
                    "create date" => ascending ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
                    _ => ascending ? query.OrderBy(p => p.ProductId) : query.OrderByDescending(p => p.ProductId),
                };

                // Count total items before pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

                var items = await query.ToListAsync(); // Execute query to retrieve paginated products
                return new PaginationResult<Product> // Return pagination result
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                // Handle exception or log error
                throw new ApplicationException("An error occurred while retrieving products.", ex);
            }
        }

        // Method for retrieving a product by its ID
        public async Task<Product?> GetProductByIdAsync(int ProductId)
        {
            try
            {
                return await _dbContext.Products.FindAsync(ProductId); // Retrieve product by ID using Entity Framework
            }
            catch (Exception ex)
            {
                // Handle exception or log error
                throw new ApplicationException($"An error occurred while retrieving product with ID {ProductId}.", ex);
            }
        }

        // Method for creating a new product
     public async Task<Product> CreateProductAsync(Product product)
        {
            try
            {
                // product.ProductId = await IdGenerator.GenerateIdAsync<Product>(_dbContext); // Generate ID for the new product
                _dbContext.Products.Add(product); // Add product to the database context
                await _dbContext.SaveChangesAsync(); // Save changes to the database
                return product; // Return the created product
            }
            catch (Exception ex)
            {
                // Handle exception or log error
                throw new ApplicationException("An error occurred while creating product.", ex);
            }
        }

        // Method for updating an existing product
        public async Task<Product?> UpdateProductAsync(int ProductId, Product product)
        {
            try
            {
                var existingProduct = await _dbContext.Products.FindAsync(ProductId); // Find the existing product by ID
                if (existingProduct != null) // Check if the product exists
                {
                    // Update product properties with new values
                    existingProduct.ProductName = product.ProductName;
                    existingProduct.ProductSlug = SlugGenerator.GenerateSlug(product.ProductName);
                    existingProduct.ProductDescription = product.ProductDescription;
                    existingProduct.ProductPrice = product.ProductPrice;
                    existingProduct.ProductImage = product.ProductImage;
                    existingProduct.ProductQuantityInStock = product.ProductQuantityInStock;
                    existingProduct.CreatedAt = product.CreatedAt;
                    existingProduct.CategoryId = product.CategoryId;

                    await _dbContext.SaveChangesAsync(); // Save changes to the database
                    return existingProduct; // Return the updated product
                }
                else
                {
                    return null; // Return null if the product does not exist
                }
            }
            catch (Exception ex)
            {
                // Handle exception or log error
                throw new ApplicationException($"An error occurred while updating product with ID {ProductId}.", ex);
            }
        }

        // Method for deleting a product by its ID
        public async Task<bool> DeleteProductAsync(int productId)
        {
            try
            {
                var productToDelete = await _dbContext.Products.FindAsync(productId); // Find the product to delete by ID
                if (productToDelete != null) // Check if the product exists
                {
                    _dbContext.Products.Remove(productToDelete); // Remove the product from the database context
                    await _dbContext.SaveChangesAsync(); // Save changes to the database
                    return true; // Return true indicating successful deletion
                }
                else
                {
                    return false; // Return false indicating that the product was not found
                }
            }
            catch (Exception ex)
            {
                // Handle exception or log error
                throw new ApplicationException($"An error occurred while deleting product with ID {productId}.", ex);
            }
        }

        // Method for searching products by keyword
        public async Task<IEnumerable<Product>> SearchProductsAsync(string keyword)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword)) // Check if the keyword is null or empty
                {
                    // If no keyword provided, return all products
                    return await _dbContext.Products.ToListAsync(); // Retrieve all products from the database
                }
                else
                {
                    // Search products based on keyword in ProductName or ProductDescription
                    return await _dbContext.Products
                        .Where(p => p.ProductName.Contains(keyword) || p.ProductDescription.Contains(keyword))
                        .ToListAsync(); // Retrieve products matching the search criteria from the database
                }
            }
            catch (Exception ex)
            {
                // Handle exception or log error
                throw new ApplicationException("An error occurred while searching products.", ex);
            }
        }

        // New method to get a product by its slug
        public async Task<Product?> GetProductBySlugAsync(string productSlug)
        {
            try
            {
                return await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductSlug == productSlug);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred while retrieving product with slug {productSlug}.", ex);
            }
        }
    }
}