using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Backend.Helpers;
using System.IdentityModel.Tokens.Jwt;
using Backend.Dtos;
using Backend.Middlewares;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        // Constructor to initialize UserService
        public UserController(UserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        // Endpoint to retrieve all Users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                bool isAdmin = true;
                if (isAdmin)
                {
                    //"Admin access granted"
                    var users = await _userService.GetAllUsersAsync();
                    return ApiResponse.Success(users, "all users are returned successfully");
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

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(int userId)
        {
            try
            {
                if (!Request.Cookies.ContainsKey("jwt"))
                {
                    throw new UnauthorizedAccessExceptions("You are not logged in ❗");
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
                        var user = await _userService.GetUserByIdAsync(userId);
                        if (user != null)
                        {
                            return ApiResponse.Success(user);
                        }
                        else
                        {
                            throw new NotFoundException("User was not found");
                        }
                    }
                    else
                    {
                        throw new UnauthorizedAccessExceptions("You don't have permission to access this endpoint");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InternalServerException(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto userDto)
        {
            try
            {
                if (!Request.Cookies.ContainsKey("jwt"))
                {
                    throw new UnauthorizedAccessExceptions("You are not logged in ❗");
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
                        var user = new User
                        {
                            FirstName = userDto.FirstName,
                            LastName = userDto.LastName,
                            Email = userDto.Email,
                            Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                            Mobile = userDto.Mobile,
                            IsAdmin = userDto.IsAdmin,
                            IsBanned = userDto.IsBanned
                        };
                        // Create the user
                        var createdUser = await _userService.CreateUserAsync(user);
                        var test = CreatedAtAction(nameof(GetUser), new { userId = createdUser.UserId }, createdUser) ?? throw new NotFoundException("Failed to create a user");
                        return ApiResponse.Created(createdUser, "User created successfully");
                    }
                    else
                    {
                        throw new UnauthorizedAccessExceptions("You don't have permission to access this endpoint");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InternalServerException(ex.Message);
            }
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, UpdateUserDto updateUserDto)
        {
            try
            {
                // Fetch the existing user from the database
                var existingUser = await _userService.GetUserByIdAsync(userId) ?? throw new NotFoundException("User was not found");

                // Check if the update request is from an admin
                bool isAdmin = updateUserDto.IsAdmin.HasValue && updateUserDto.IsAdmin.Value;

                if (isAdmin)
                {
                    // Admin can update all fields
                    if (updateUserDto.FirstName != null)
                    {
                        existingUser.FirstName = updateUserDto.FirstName;
                    }
                    if (updateUserDto.LastName != null)
                    {
                        existingUser.LastName = updateUserDto.LastName;
                    }
                    if (updateUserDto.Email != null)
                    {
                        existingUser.Email = updateUserDto.Email;
                    }
                    if (updateUserDto.Mobile != null)
                    {
                        existingUser.Mobile = updateUserDto.Mobile;
                    }
                    if (updateUserDto.Password != null)
                    {
                        existingUser.Password = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
                    }
                    if (updateUserDto.IsAdmin.HasValue)
                    {
                        existingUser.IsAdmin = updateUserDto.IsAdmin.Value;
                    }
                    if (updateUserDto.IsBanned.HasValue)
                    {
                        existingUser.IsBanned = updateUserDto.IsBanned.Value;
                    }
                }
                else
                {
                    // Regular user can update only FirstName, LastName, and Mobile
                    if (updateUserDto.FirstName != null)
                    {
                        existingUser.FirstName = updateUserDto.FirstName;
                    }
                    if (updateUserDto.LastName != null)
                    {
                        existingUser.LastName = updateUserDto.LastName;
                    }
                }

                // Save the updated user
                var updatedUser = await _userService.UpdateUserAsync(userId, existingUser);
                if (updatedUser == null)
                {
                    throw new NotFoundException("User was not found");
                }
                else
                {
                    return ApiResponse.Success(updatedUser, "User updated successfully");
                }
            }
            catch (Exception ex)
            {
                throw new InternalServerException(ex.Message);
            }
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                bool isAdmin = true;

                if (isAdmin)
                {
                    // Check if the User with userId exists
                    var existingUser = await _userService.GetUserByIdAsync(userId) ?? throw new NotFoundException($"User with ID {userId} was not found.");

                    //"Admin access granted"
                    var result = await _userService.DeleteUserAsync(userId);
                    if (result)
                    {
                        return ApiResponse.Deleted(existingUser, $"User with ID {userId} successfully deleted.");
                    }
                    else
                    {
                        // User was not found, return a success response since the deletion operation succeeded (from the client's perspective).
                        throw new InternalServerException($"Failed to delete user with ID {userId}.");
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