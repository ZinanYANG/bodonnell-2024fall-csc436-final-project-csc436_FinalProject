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

// Define login endpoint with the updated RedirectUri pointing to your deployed frontend
app.MapGet("/login", async context =>
{
    await context.ChallengeAsync("Google", new AuthenticationProperties
    {
        RedirectUri = "https://nice-moss-014326b10.5.azurestaticapps.net/account"  // Update this URL to match your deployed frontend's account page
    });
});

app.MapGet("/logout", async context =>
{
    await context.SignOutAsync("Cookies");
    context.Response.Redirect("https://nice-moss-014326b10.5.azurestaticapps.net"); // Redirect to the homepage or login page on the deployed frontend
}).WithName("Logout");

// Other endpoints remain the same
// ... (rest of your endpoints)

app.Run();

// Record for WeatherForecast, used by the example weather endpoint
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
