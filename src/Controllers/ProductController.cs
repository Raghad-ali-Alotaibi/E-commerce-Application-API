using api.Controllers;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Backend.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts(
            [FromQuery] string? productName = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] DateTime? createDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "id",
            [FromQuery] bool ascending = true)
        {
            try
            {
                var sortOptions = new List<string> { "id", "price", "title", "product name", "create date" };

                // Check if the sortBy value is valid, if not, return bad request
                if (!sortOptions.Contains(sortBy.ToLower()))
                {
                    return BadRequest("Invalid value for sortBy. Valid options are: id, price, productName, createDate (Make sure to the lower and capital letters)");
                }

                var products = await _productService.GetAllProductsAsync(productName, minPrice, maxPrice, createDate, sortBy, ascending, pageNumber, pageSize);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product != null)
                {
                    return ApiResponse.Created(product);
                }
                else
                {
                    return ApiResponse.NotFound("Product was not found");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse.ServerError(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            try
            {
                if (product == null)
                {
                    return ApiResponse.BadRequest("Product data is null");
                }

                product.ProductSlug = SlugGenerator.GenerateSlug(product.ProductName);
                var createdProduct = await _productService.CreateProductAsync(product);
                return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.ProductId }, createdProduct);
            }
            catch (Exception ex)
            {
                return ApiResponse.ServerError(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            try
            {
                if (product == null || product.ProductId != id)
                {
                    return ApiResponse.BadRequest("Invalid product data");
                }

                product.ProductSlug = SlugGenerator.GenerateSlug(product.ProductName);
                var updatedProduct = await _productService.UpdateProductAsync(id, product);
                if (updatedProduct == null)
                {
                    return ApiResponse.NotFound("Product was not found");
                }

                return ApiResponse.Success(updatedProduct, "Update product successfully");

            }
            catch (Exception ex)
            {
                return ApiResponse.ServerError(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (result)
                {
                    return NoContent();
                }
                else
                {
                    return ApiResponse.NotFound("Product was not found");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse.ServerError(ex.Message);
            }
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(string keyword)
        {
            try
            {
                var products = await _productService.SearchProductsAsync(keyword);
                return ApiResponse.Success(products, "Products are returned successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse.ServerError(ex.Message);
            }
        }
    }
}