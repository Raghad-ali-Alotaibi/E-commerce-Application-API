using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        public string ProductName { get; set; } = null!;
        [Required(ErrorMessage = "Product slug is required")]
        [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Product slug can only contain lowercase letters, numbers, and hyphens.")]
        public string? ProductSlug { get; set; }
        [Required(ErrorMessage = "Product description is required")]
        public string ProductDescription { get; set; } = null!;

        [Required(ErrorMessage = "Product price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Product price must be greater than 0")]
        public decimal ProductPrice { get; set; }

        [Url(ErrorMessage = "Invalid URL format")]
        public string? ProductImage { get; set; }

        [Required(ErrorMessage = "Product quantity in stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Product quantity must be a positive number")]
        public int ProductQuantityInStock { get; set; }
        [JsonIgnore]
        public DateTime? CreatedAt { get; set; }
         

        public int? CategoryId { get; set; }
        [JsonIgnore]
        public virtual Category? Category { get; set; }

        [JsonIgnore]
        public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    }
}