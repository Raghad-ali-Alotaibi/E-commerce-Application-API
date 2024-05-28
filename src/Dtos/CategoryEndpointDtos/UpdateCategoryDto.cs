using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Dtos
{
    public class UpdateCategoryDto
    {
        public int CategoryId { get; set; }
        [MaxLength(50, ErrorMessage = "CategoryName must be less than 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "CategoryName must contain only letters")]
        public string? CategoryName { get; set; }
        [MaxLength(100, ErrorMessage = "CategoryDescription must be less than 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "CategoryDescription must contain only letters")]
        public string? CategoryDescription { get; set; }
    }
}