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


builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

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
    return client.GetDatabase(settings.DatabaseName);
});

// Register your services with DI
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<UserService>();

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

    // Add debug log to verify token validation
    options.Events = new JwtBearerEvents
    {
    OnTokenValidated = context =>
    {
        // Debugging token validation
        var userId = context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = context.Principal.FindFirst(ClaimTypes.Role)?.Value;

        Console.WriteLine($"Token Validated for User ID: {userId}, Role: {userRole}");
        
        return Task.CompletedTask;
    },
    OnAuthenticationFailed = context =>
    {
        // Log authentication failure
        Console.WriteLine($"Authentication Failed: {context.Exception.Message}");
        
        // Optional: Return custom error response for unauthenticated users
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new { message = "Authentication failed" });
            return context.Response.WriteAsync(result);
        }

        return Task.CompletedTask;
    },
    OnChallenge = context =>
    {
        // This is invoked when the user is not authenticated
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new { message = "You are not authorized" });
            return context.Response.WriteAsync(result);
        }

        return Task.CompletedTask;
    },
    OnForbidden = context =>
    {
        // This is invoked when the user is authenticated but does not have the required roles
        context.Response.StatusCode = 403;
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new { message = "You do not have sufficient privileges" });
        return context.Response.WriteAsync(result);
    }
    };



    // Adjust the TokenValidationParameters to skip issuer and audience validation
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ValidateIssuer = false, // Disable issuer validation
        ValidateAudience = false, // Disable audience validation
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role // Use ClaimTypes.Role to match the role claim in the token
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

// Use Routing
app.UseRouting();

// ** Start of New Code for User Authentication Middleware ** //
app.UseAuthentication(); // Add JWT Authentication Middleware
app.UseAuthorization();  // Add Authorization Middleware
// ** End of New Code for User Authentication Middleware ** //

app.MapControllers();

app.Run();
