using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos
{
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 50 characters.")]
        public string CategoryName { get; set; } = null!;

        // [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Category slug can only contain lowercase letters, numbers, and hyphens.")]
        // public string? CategorySlug { get; set; }

        [StringLength(500, ErrorMessage = "Category description cannot exceed 500 characters.")]
        public string CategoryDescription { get; set; } = null!;
    }
}
