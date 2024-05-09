using Backend.Dtos;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Backend.Helpers;
using System.IdentityModel.Tokens.Jwt;


namespace Backend.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        // Constructor to initialize CategoryService
        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        }

        // Endpoint to retrieve all categories
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                // Call service to get all categories
                var categories = await _categoryService.GetAllCategories();
                return ApiResponse.Success(categories, "all categories retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse.ServerError(ex.Message);
            }
        }

        // Endpoint to retrieve a specific category by ID
        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetCategory(int categoryId)
        {
            try
            {
                // Call service to get category by ID
                var category = await _categoryService.GetCategoryById(categoryId);
                if (category != null)
                {
                    return ApiResponse.Created(category);
                }
                else
                {
                    return ApiResponse.NotFound("Category was not found");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse.ServerError(ex.Message);
            }
        }

        // Endpoint to create a new category
        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            try
            {
                if (!Request.Cookies.ContainsKey("jwt"))
                {
                    return Unauthorized("Not have any token to access");
                }
                else
                {
                    var jwt = Request.Cookies["jwt"];
                    // Validate and decode JWT token to extract claims
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var token = tokenHandler.ReadJwtToken(jwt);

                    var isAdminClaim = token.Claims.FirstOrDefault(c => c.Type == "role" && c.Value == "Admin");

                    bool isAdmin = isAdminClaim != null;

                    if (isAdmin)
                    {
                        //"Admin access granted"
                        // Generate slug and create new category
                        category.CategorySlug = SlugGenerator.GenerateSlug(category.CategoryName);
                        var createdCategory = await _categoryService.CreateCategory(category);
                        var test = CreatedAtAction(nameof(GetCategory), new { id = createdCategory.CategoryId }, createdCategory);
                        if (test == null)
                        {
                            return ApiResponse.NotFound("Failed to create a category");
                        }
                        return ApiResponse.Created("Category created successfully");
                    }
                    else
                    {
                        return Unauthorized("You don't have permission to access this endpoint");
                    }
                }
            }
            catch (Exception ex)
            {
                return ApiResponse.ServerError(ex.Message);
            }
        }

        // Endpoint to update an existing category
        [HttpPut("{categoryId}")]
        public async Task<IActionResult> UpdateCategory(int categoryId, UpdateCategoryDto categoryDto)
        {
            try
            {
                if (!Request.Cookies.ContainsKey("jwt"))
                {
                    return Unauthorized("Not have any token to access");
                }
                else
                {
                    var jwt = Request.Cookies["jwt"];
                    // Validate and decode JWT token to extract claims
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var token = tokenHandler.ReadJwtToken(jwt);

                    var isAdminClaim = token.Claims.FirstOrDefault(c => c.Type == "role" && c.Value == "Admin");

                    bool isAdmin = isAdminClaim != null;

                    if (isAdmin)
                    {
                        //"Admin access granted"
                        // Call service to update category
                        var updatedCategory = await _categoryService.UpdateCategory(categoryId, categoryDto);
                        if (updatedCategory == null)
                        {
                            return ApiResponse.NotFound("Category was not found");
                        }
                        else
                        {
                            return ApiResponse.Success(updatedCategory, "Update Category successfully");
                        }
                    }
                    else
                    {
                        return Unauthorized("You don't have permission to access this endpoint");
                    }
                }
            }
            catch (Exception ex)
            {
                return ApiResponse.ServerError(ex.Message);
            }
        }

        // Endpoint to delete a category by ID
        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                if (!Request.Cookies.ContainsKey("jwt"))
                {
                    return Unauthorized("Not have any token to access");
                }
                else
                {
                    var jwt = Request.Cookies["jwt"];
                    // Validate and decode JWT token to extract claims
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var token = tokenHandler.ReadJwtToken(jwt);

                    var isAdminClaim = token.Claims.FirstOrDefault(c => c.Type == "role" && c.Value == "Admin");

                    bool isAdmin = isAdminClaim != null;

                    if (isAdmin)
                    {
                        //"Admin access granted"
                        // Call service to delete category
                        var result = await _categoryService.DeleteCategory(categoryId);
                        if (result)
                        {
                            return NoContent();
                        }
                        else
                        {
                            return ApiResponse.NotFound("Category was not found");
                        }
                    }
                    else
                    {
                        return Unauthorized("You don't have permission to access this endpoint");
                    }
                }
            }
            catch (Exception ex)
            {
                return ApiResponse.ServerError(ex.Message);
            }
        }
    }
}
