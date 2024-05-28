using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Backend.Dtos;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;


namespace auth.Helpers
{
    public class JwtService
    {

        public JwtService()
        {
            Console.WriteLine($"Jwt__Key: {Environment.GetEnvironmentVariable("Jwt__Key")}");
        }

        public string GenerateJwt(UserDto user)
        {
            // Retrieve the JWT key from configuration
            var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key") ?? throw new InvalidOperationException("JWT Key is missing in environment variables.");
            var jwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer") ?? throw new InvalidOperationException("JWT Issuer is missing in environment variables.");
            var jwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience") ?? throw new InvalidOperationException("JWT Audience is missing in environment variables.");
            // Check if the key is null or empty
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT key is not configured properly.");
            }

            // Convert the key to bytes
            var key = Encoding.ASCII.GetBytes(jwtKey);

            // Check if the key is null
            if (key == null || key.Length == 0)
            {
                throw new InvalidOperationException("JWT key is not configured properly.");
            }

            // Determine if the user is an admin
            bool isAdmin = user.IsAdmin ?? false;

            // Create a token descriptor specifying the claims, expiration, and signing credentials
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // Define the claims for the token
                Subject = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // User ID
            new Claim(ClaimTypes.Role, isAdmin? "Admin" : "User"), // Role (Admin/User)
        }),

                // Set the expiration time for the token (e.g., 2 minutes from now)
                Expires = DateTime.UtcNow.AddMinutes(2),

                // Specify the signing credentials using the JWT key
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),

                // Set the issuer and audience of the token
                Issuer = jwtIssuer,
                Audience = jwtAudience,
            };

            // Create a JWT token handler
            var tokenHandler = new JwtSecurityTokenHandler();

            // Generate the JWT token
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Write the JWT token as a string
            return tokenHandler.WriteToken(token);
        }
    }
}