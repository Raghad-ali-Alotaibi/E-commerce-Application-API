using auth.Data;
using Backend.Dtos;
using auth.Helpers;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using auth.Dtos;
using Backend.Middlewares;
using Backend.Helpers;



namespace auth.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IUserRepository _repository;
        private readonly JwtService _jwtService;
        // Constructor
        public AuthController(IUserRepository repository, JwtService jwtService)
        {
            _repository = repository;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            // Create a new User object using the data from the RegisterDto
            var user = new User
            {
                FirstName = dto.FirstName ?? "", // Set the first name from the DTO (or an empty string if null)
                LastName = dto.LastName ?? "",   // Set the last name from the DTO (or an empty string if null)
                Email = dto.Email ?? "",         // Set the email from the DTO (or an empty string if null)                                
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password), // Hash the password using BCrypt.Net and set it in the user object
                Mobile = dto.Mobile    // Set the mobile number from the DTO
            };

            // Create the user in the repository and return the result with a "Created" status code
            return ApiResponse.Created(_repository.Create(user), "Successfully Registered");
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            try
            {
                // Retrieve the user from the repository by email
                var user = _repository.GetByEmail(dto.Email ?? "");

                if (user == null || user.Email == "")
                {
                    throw new BadRequestException("Invalid Email ❗");
                }

                // Verify the password using BCrypt.Net
                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                {
                    // If the password is invalid, return a BadRequest response
                    throw new BadRequestException("Invalid Password ❗");
                }

                // Create a UserDto object from the retrieved user
                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Mobile = user.Mobile,
                    IsAdmin = user.IsAdmin,
                    IsBanned = user.IsBanned,
                };

                // Generate a JWT token for the authenticated user
                var jwt = _jwtService.GenerateJwt(userDto) ?? throw new ConflictException("Don't have sufficient permissions to access the requested resource");

                // Store the JWT and UserDto in a variable
                var data = new
                {
                    Jwt = jwt,
                    UserDto = userDto
                };

                // Add the JWT token to a cookie for future authentication
                Response.Cookies.Append("jwt", jwt, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Set to true if using HTTPS
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(1) // Set your desired expiry time 
                });

                // Return a successful response with a welcome message
                //    return ApiResponse.Success($"Successfully logged in, welcome back again {userDto.FirstName} 🌸 ");
                // Return a successful response with a userDto
                // return ApiResponse.Created(userDto, "Successfully logged in");
                return ApiResponse.Created(data, "Successfully logged in");
            }
            catch (Exception e)
            {
                // If an exception occurs during login, return a BadRequest response with the error message
                throw new InternalServerException($"{e.Message}");
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Check if the "jwt" cookie exists in the request
            if (Request.Cookies.ContainsKey("jwt"))
            {
                // If the cookie exists, delete it from the response
                Response.Cookies.Delete("jwt");

                // Return a successful response indicating successful logout
                return ApiResponse.Success("Logged out successfully ✔️");
            }
            else
            {
                // If the "jwt" cookie does not exist, return a NotFound response
                throw new UnauthorizedAccessExceptions("You are not logged in ❗");
            }
        }
    }
}