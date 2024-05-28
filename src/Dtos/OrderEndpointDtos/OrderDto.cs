

namespace Backend.Dtos
{
        public class OrderDto
        {
                public int OrderId { get; set; }
                public DateTime? OrderDate { get; set; }
                public string OrderStatus { get; set; } = null!;
                public string? Payment { get; set; } = null!;
                public int? UserId { get; set; }

                public UserDto User { get; set; } = null!;

                public ICollection<OrderProductDto> OrderProducts { get; set; } = null!; // Include OrderProduct collection
        }
}