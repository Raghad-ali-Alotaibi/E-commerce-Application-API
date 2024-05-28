using auth.Data;
using auth.Helpers;
using Backend.Helpers;
using Backend.Middlewares;
using Backend.Models;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

/**

Set up the infrastructure for a backend API, including service registration, 
database context configuration, middleware setup, and controller mapping and integrate 
Swagger for API documentation in the development environment, with CORS configuration 
and error handling middleware.
*/

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
DotNetEnv.Env.Load();
// Get JWT settings from environment variables
var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key") ?? throw new InvalidOperationException("JWT Key is missing in environment variables.");
var jwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer") ?? throw new InvalidOperationException("JWT Issuer is missing in environment variables.");
var jwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience") ?? throw new InvalidOperationException("JWT Audience is missing in environment variables.");
// Get the database connection string from environment variables
var defaultConnection = Environment.GetEnvironmentVariable("DefaultConnection") ?? throw new InvalidOperationException("Default Connection is missing in environment variables.");

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Backend Teamwork API", Version = "v1" });
    c.AddSwaggerExamples();
});

builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<JwtService>();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.WithOrigins("http://localhost:3000", "https://merry-unicorn-584ddd.netlify.app/")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.AddControllers();

// Database context configuration
builder.Services.AddDbContext<EcommerceSdaContext>(options =>
    options.UseNpgsql(defaultConnection));

var app = builder.Build();

// Development specific configurations
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend Teamwork API V1");
    });

    
}

app.MapGet("/", () => {
    return "Welcome to E-commerce Application API";
}).WithOpenApi();

// Middleware setup
app.UseHttpsRedirection();

// Authentication and Authorization setup
app.UseAuthentication();
app.UseAuthorization();

// CORS setup
app.UseCors("CorsPolicy");

// Error handling middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("An unexpected error occurred.");
    });
});
app.UseMiddleware<ExceptionHandling>();

// Finalize setup and start listening for incoming requests
app.MapControllers();
app.Run();