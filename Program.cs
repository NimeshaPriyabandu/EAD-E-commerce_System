using E_commerce_system.Configurations;
using E_commerce_system.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Your React app URL
              .AllowAnyHeader()
              .AllowAnyMethod();
            //   .AllowCredentials(); // If you need to allow credentials like cookies or auth headers
    });
});

builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

// Register MongoDB client
builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

// Register IMongoDatabase with DI
builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    return client.GetDatabase(settings.DatabaseName); // Resolve IMongoDatabase
});

// Register ProductService, OrderService, and UserService
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<VendorService>();

// ** Start of New Code for User Authentication ** //

// Load JWT settings from appsettings.json
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Key))
{
    throw new InvalidOperationException("JWT Key is missing or empty in configuration.");
}

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.Events = new JwtBearerEvents
    {
        // Token successfully validated
        OnTokenValidated = context =>
        {
            var userId = context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = context.Principal.FindFirst(ClaimTypes.Role)?.Value;

            Console.WriteLine($"Token Validated for User ID: {userId}, Role: {userRole}");
            return Task.CompletedTask;
        },
        
        // Handle authentication failures, e.g., invalid token
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication Failed: {context.Exception.Message}");

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                
                // Renamed result to authFailedResult
                var authFailedResult = System.Text.Json.JsonSerializer.Serialize(new { message = "Authentication failed. Please ensure your token is valid." });
                return context.Response.WriteAsync(authFailedResult);
            }

            return Task.CompletedTask;
        },
        
        // Handle the scenario where no token is provided
        OnChallenge = context =>
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                
                // Renamed result to challengeResult
                var challengeResult = System.Text.Json.JsonSerializer.Serialize(new { message = "Authentication required. Please provide a valid token." });
                return context.Response.WriteAsync(challengeResult);
            }

            return Task.CompletedTask;
        },
        
        // Handle cases where a valid token is provided but the user does not have sufficient privileges
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            
            // Renamed result to forbiddenResult
            var forbiddenResult = System.Text.Json.JsonSerializer.Serialize(new { message = "Access denied. You do not have sufficient permissions to perform this action." });
            return context.Response.WriteAsync(forbiddenResult);
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ValidateIssuer = false, 
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role
    };
});


// Add Role-Based Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("VendorOnly", policy => policy.RequireRole("Vendor"));
    options.AddPolicy("CSROnly", policy => policy.RequireRole("CSR"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
});

// ** End of New Code for User Authentication ** //

// Add controllers service
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontendApp");

// Use Routing
app.UseRouting();

// ** Start of New Code for User Authentication Middleware ** //
app.UseAuthentication(); // Add JWT Authentication Middleware
app.UseAuthorization();  // Add Authorization Middleware
// ** End of New Code for User Authentication Middleware ** //

app.MapControllers();

app.Run();
