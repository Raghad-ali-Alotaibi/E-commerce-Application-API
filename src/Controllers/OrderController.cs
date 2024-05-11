using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Backend.Helpers;
using Backend.Dtos;
using System.IdentityModel.Tokens.Jwt;

namespace api.Controllers
{
    [ApiController]
    [Route("/api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        // to initialize the services we will create constructor 
        public OrderController(OrderService orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService)); ;
        }

        // help to make get request
        [HttpGet]
        //IActionResult give all status code like :ok,not found.....
        public async Task<IActionResult> GetAllOrder()
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
                    // Check if user has admin role

                    var isAdminClaim = token.Claims.FirstOrDefault(c => c.Type == "role" && c.Value == "Admin");

                    bool isAdmin = isAdminClaim != null;

                    if (isAdmin)
                    {
                        //"Admin access granted" 
                        // Retrieve all orders using OrderService
                        var orders = await _orderService.GetAllOrdersService();
                        return ApiResponse.Success(orders, "All Orders returned successfully");
                    }
                    else
                    {
                        return Unauthorized("You don't have permission to access this endpoint");
                    }
                }
            }
            catch (Exception e)
            {
                return ApiResponse.BadRequest(e.Message);
            }
        }
        // GET endpoint to retrieve a specific order by ID
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            try
            {
                // Check if JWT token exists in request cookies

                if (!Request.Cookies.ContainsKey("jwt"))
                {
                    // If JWT token is not found, return unauthorized response
                    return Unauthorized("Not have any token to access");
                }
                else
                {
                    var jwt = Request.Cookies["jwt"];
                    // Validate and decode JWT token to extract claims
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var token = tokenHandler.ReadJwtToken(jwt);
                    // Check if user has admin role
                    var isAdminClaim = token.Claims.FirstOrDefault(c => c.Type == "role" && c.Value == "Admin");

                    bool isAdmin = isAdminClaim != null;

                    if (isAdmin)
                    {
                        //"Admin access granted"
                        // Retrieve all orders using OrderService
                        var order = await _orderService.GetOrderByIdService(orderId);
                        if (order != null)
                        {
                            return ApiResponse.Success(order, "Order returned successfully");
                        }
                        else
                        {
                            // If order is not found, return not found response
                            return ApiResponse.NotFound("order not found");
                        }
                    }
                    else
                    {
                        // If user is not an admin, return unauthorized response

                        return Unauthorized("You don't have permission to access this endpoint");
                    }
                }
            }
            catch (Exception ex)
            {
                return ApiResponse.ServerError(ex.Message);
            }
        }
        // POST endpoint to create a new order

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreationDto orderCreationDto)
        {
            try
            {
                // Check if JWT token exists in request cookies
                if (!Request.Cookies.ContainsKey("jwt"))
                {
                    // If JWT token is not found, return unauthorized response
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
                        // Create new order using OrderService
                        var createdOrder = await _orderService.CreateOrderService(orderCreationDto.NewOrder, orderCreationDto.Products);
                        // Check if order creation was successful
                        var test = CreatedAtAction(nameof(GetOrder), new { orderId = createdOrder.OrderId }, createdOrder);
                        if (test == null)
                        {
                            // If order creation failed, return not found response
                            return ApiResponse.NotFound("Failed to create an order");
                        }
                        // If order creation was successful, return created response

                        return ApiResponse.Created(orderCreationDto, "Order created successfully");
                    }
                    else
                    {
                        // If user is not an admin, return unauthorized response

                        return Unauthorized("You don't have permission to access this endpoint");
                    }
                }
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
        // PUT endpoint to update an existing order
        [HttpPut("{OrderId}")]
        public async Task<IActionResult> UpdateOrder(int OrderId, Order order)
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
                        // Update order using OrderService
                        var updatedOrder = await _orderService.UpdateOrderService(OrderId, order);
                        if (updatedOrder == null)
                        {
                            // If order was not found, return not found response
                            return NotFound("Order was not found");

                        }
                        else
                        {
                            // If order was updated successfully, return OK response

                            return Ok("Update Order successfully");
                        }
                    }
                    else
                    {
                        // If user is not an admin, return unauthorized response

                        return Unauthorized("You don't have permission to access this endpoint");
                    }
                }
            }
            catch (Exception ex)
            {
                return ApiResponse.ServerError(ex.Message);
            }
        }
        // DELETE endpoint to delete an order

        [HttpDelete("{orderId}")]

        public async Task<IActionResult> DeleteOrder(int orderId)
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
                        // Check if the order with orderId exists
                        var existingOrder = await _orderService.GetOrderByIdService(orderId);
                        if (existingOrder == null)
                        {
                            // If order does not exist, return not found response

                            return NotFound("Order not found.");
                        }
                        // Delete order using OrderService

                        var result = await _orderService.DeleteOrderService(orderId);
                        if (!result)
                        {
                            // If deletion failed, return not found response

                            return ApiResponse.NotFound($"Failed to delete order with ID {orderId}.");
                        }
                        else
                        {
                            // If deletion was successful, return no content response

                            return NoContent();
                        }
                    }
                    else
                    {
                        // If user is not an admin, return unauthorized response

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

