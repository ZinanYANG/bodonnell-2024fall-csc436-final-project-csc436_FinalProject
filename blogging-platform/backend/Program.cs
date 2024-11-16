using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using MongoDB.Driver;
using MongoDB.Bson;

var connectionString = "mongodb+srv://dbUser:15947150490@csc436cluster.wxaji.mongodb.net/BloggingPlatformDb?retryWrites=true&w=majority";
var databaseName = "BloggingPlatformDb";

var builder = WebApplication.CreateBuilder(args);

// Configure CORS policy to allow requests from the frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder =>
        {
            builder.WithOrigins("https://nice-moss-014326b10.5.azurestaticapps.net")
                   .AllowCredentials()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Register MongoDbContext as a Singleton service
builder.Services.AddSingleton(new MongoDbContext(connectionString, databaseName));

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services for Google Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "Google";
})
.AddCookie("Cookies") // Enables cookie-based session management
.AddGoogle("Google", options =>
{
    options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
    options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
    options.CallbackPath = "/signin-google";  // This must match the URI set in Google Console

    // Explicitly request email and profile scopes
    options.Scope.Add("email");
    options.Scope.Add("profile");

    // Map the standard Google claims to custom names
    options.ClaimActions.MapJsonKey("urn:google:email", "email");
    options.ClaimActions.MapJsonKey("urn:google:name", "name");
});

// Add authorization service
builder.Services.AddAuthorization();

var app = builder.Build();

// Apply CORS policy before using authentication and authorization
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Example API endpoint
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// Replace all localhost URLs with your actual deployed URLs
app.MapGet("/api/blogposts/{id}", async (MongoDbContext dbContext, string id) =>
{
    var blogPost = await dbContext.BlogPosts.Find(post => post.Id == id).FirstOrDefaultAsync();
    return blogPost is not null ? Results.Ok(blogPost) : Results.NotFound();
})
.WithName("GetBlogPostById");

app.MapGet("/api/user/blogposts", async (HttpContext context, MongoDbContext dbContext) =>
{
    var userEmail = context.User.FindFirst("urn:google:email")?.Value;
    if (string.IsNullOrEmpty(userEmail)) return Results.Unauthorized();

    var user = await dbContext.Users.Find(u => u.Email == userEmail).FirstOrDefaultAsync();
    if (user == null) return Results.NotFound("User not found.");

    var userPosts = await dbContext.BlogPosts.Find(post => post.AuthorId == user.Id).ToListAsync();
    return Results.Ok(userPosts);
});

app.MapPost("/api/user/blogposts", async (HttpContext context, MongoDbContext dbContext, BlogPost newPost) =>
{
    var userEmail = context.User.FindFirst("urn:google:email")?.Value;
    if (string.IsNullOrEmpty(userEmail)) return Results.Unauthorized();

    var user = await dbContext.Users.Find(u => u.Email == userEmail).FirstOrDefaultAsync();
    if (user == null) return Results.NotFound("User not found.");

    newPost.AuthorId = user.Id;
    await dbContext.BlogPosts.InsertOneAsync(newPost);
    return Results.Created($"/api/user/blogposts/{newPost.Id}", newPost);
});

app.MapGet("/api/blogposts", async (MongoDbContext dbContext) =>
{
    var blogPosts = await dbContext.BlogPosts.Find(_ => true).ToListAsync();
    return Results.Ok(blogPosts);
})
.WithName("GetAllBlogPosts");

app.MapGet("/login", async context =>
{
    await context.ChallengeAsync("Google", new AuthenticationProperties
    {
        RedirectUri = "https://nice-moss-014326b10.5.azurestaticapps.net/account"
    });
});

app.MapGet("/logout", async context =>
{
    await context.SignOutAsync("Cookies");
    context.Response.Redirect("https://nice-moss-014326b10.5.azurestaticapps.net"); // Redirect to the homepage or login page on the frontend
}).WithName("Logout");

app.Run();

// Record for WeatherForecast, used by the example weather endpoint
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
