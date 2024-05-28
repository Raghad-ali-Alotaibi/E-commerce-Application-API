using Backend.Dtos;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Backend.Helpers;
using System.IdentityModel.Tokens.Jwt;
using Backend.Middlewares;


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
                throw new InternalServerException(ex.Message);
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
                    throw new NotFoundException("Category was not found");
                }
            }
            catch (Exception ex)
            {
                throw new InternalServerException(ex.Message);
            }
        }

        // Endpoint to create a new category
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            try
            {
                bool isAdmin = true;

                if (isAdmin)
                {
                    //"Admin access granted"
                    // Generate slug and create new category
                    var category = new Category
                    {
                        CategoryName = createCategoryDto.CategoryName,
                        CategorySlug = SlugGenerator.GenerateSlug(createCategoryDto.CategoryName),
                        CategoryDescription = createCategoryDto.CategoryDescription
                    };

                    var createdCategory = await _categoryService.CreateCategory(category);
                    var test = CreatedAtAction(nameof(GetCategory), new { categoryId = createdCategory.CategoryId }, createdCategory) ?? throw new NotFoundException("Failed to create a category");
                    return ApiResponse.Created(createCategoryDto, "Category created successfully");
                }
                else
                {
                    throw new UnauthorizedAccessExceptions("You don't have permission to access this endpoint");
                }

            }
            catch (Exception ex)
            {
                throw new InternalServerException(ex.Message);
            }
        }
      
        // Endpoint to update an existing category
        [HttpPut("{categoryId}")]
        public async Task<IActionResult> UpdateCategory(int categoryId, UpdateCategoryDto categoryDto)
        {
            try
            {
                bool isAdmin = true;
                if (isAdmin)
                {
                    //"Admin access granted"
                    // Call service to update category
                    var updatedCategory = await _categoryService.UpdateCategory(categoryId, categoryDto);
                    if (updatedCategory == null)
                    {
                        throw new NotFoundException("Category was not found");
                    }
                    else
                    {
                        return ApiResponse.Success(updatedCategory, "Update Category successfully");
                    }
                }
                else
                {
                    throw new UnauthorizedAccessExceptions("You don't have permission to access this endpoint");
                }

            }
            catch (Exception ex)
            {
                throw new InternalServerException(ex.Message);
            }
        }


        // Endpoint to delete a category by ID
        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                bool isAdmin = true;
                if (isAdmin)
                {
                    // Check if the category with categorytId exists
                    var existingCategory = await _categoryService.GetCategoryById(categoryId) ?? throw new NotFoundException($"Category with ID {categoryId} was not found");

                    //"Admin access granted"
                    // Call service to delete category
                    var result = await _categoryService.DeleteCategory(categoryId);
                    if (result)
                    {
                        return ApiResponse.Deleted(existingCategory, $"Category with ID {categoryId} successfully deleted.");
                    }
                    else
                    {
                        throw new InternalServerException($"Failed to delete category with ID {categoryId}");
                    }
                }
                else
                {
                    throw new UnauthorizedAccessExceptions("You don't have permission to access this endpoint");
                }

            }
            catch (Exception ex)
            {
                throw new InternalServerException(ex.Message);
            }
        }
    }
}
