using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using MongoDB.Driver;
using MongoDB.Bson;
using DotNetEnv;
using Microsoft.Extensions.Logging;

// Load environment variables
DotNetEnv.Env.Load();

// Environment variables
var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") ??
    "mongodb+srv://dbUser:15947150490@csc436cluster.wxaji.mongodb.net/BloggingPlatformDb?retryWrites=true&w=majority";
var databaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME") ?? "BloggingPlatformDb";
var frontendBaseUrl = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL") ?? "http://localhost:3000";
var googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
var googleClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
var redirectUri = Environment.GetEnvironmentVariable("FRONTEND_ACCOUNT_PAGE") ?? $"{frontendBaseUrl}/account";

// Logging checks
Console.WriteLine("Starting environment variable checks...");
void LogEnvVar(string name, string? value, bool hide = false)
{
    Console.WriteLine($"{name}: {(hide && !string.IsNullOrEmpty(value) ? "(hidden for security)" : value)}");
}

LogEnvVar("MONGO_CONNECTION_STRING", connectionString, hide: true);
LogEnvVar("MONGO_DATABASE_NAME", databaseName);
LogEnvVar("FRONTEND_BASE_URL", frontendBaseUrl);
LogEnvVar("GOOGLE_CLIENT_ID", googleClientId);
LogEnvVar("GOOGLE_CLIENT_SECRET", googleClientSecret, hide: true);
LogEnvVar("FRONTEND_ACCOUNT_PAGE", redirectUri);

// MongoDB connection
var client = new MongoClient(connectionString);
var database = client.GetDatabase(databaseName);

try
{
    var collections = database.ListCollectionNames().ToList();
    Console.WriteLine($"Connected to MongoDB! Collections: {string.Join(", ", collections)}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error connecting to MongoDB: {ex.Message}");
    throw;
}

// Web app builder setup
var builder = WebApplication.CreateBuilder(args);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", corsBuilder =>
    {
        corsBuilder.WithOrigins(frontendBaseUrl)
                   .AllowCredentials()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
    });
});

// Services setup
builder.Services.AddSingleton(new MongoDbContext(connectionString, databaseName));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Google";
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.Name = "BloggingPlatformAuth";
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
})
.AddGoogle("Google", options =>
{
    options.ClientId = googleClientId;
    options.ClientSecret = googleClientSecret;
    options.CallbackPath = "/signin-google";
    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.ClaimActions.MapJsonKey("urn:google:email", "email");
    options.ClaimActions.MapJsonKey("urn:google:name", "name");
});

// Authorization setup
builder.Services.AddAuthorization();

var app = builder.Build();

// Apply CORS
app.UseCors("AllowReactApp");

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Swagger setup
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirect logic for authentication
redirectUri = Environment.GetEnvironmentVariable("FRONTEND_ACCOUNT_PAGE") ?? $"{frontendBaseUrl}/account";
Console.WriteLine($"Redirect URI: {redirectUri}");

app.UseHttpsRedirection();

// Routes
app.MapGet("/", () => "Welcome to the backend of Blogging Platform!");
app.MapGet("/api/blogposts", async (MongoDbContext dbContext) =>
{
    var blogPosts = await dbContext.BlogPosts.Find(_ => true).ToListAsync();
    return Results.Ok(blogPosts);
});

// Authentication routes
app.MapGet("/login", async context =>
{
    await context.ChallengeAsync("Google", new AuthenticationProperties { RedirectUri = redirectUri });
});
app.MapGet("/logout", async context =>
{
    await context.SignOutAsync("Cookies");
    context.Response.Redirect(frontendBaseUrl);
});
app.MapGet("/profile", async (HttpContext context, MongoDbContext dbContext) =>
{
    var user = context.User;
    if (user?.Identity?.IsAuthenticated ?? false)
    {
        var email = user.FindFirst("urn:google:email")?.Value ?? "no-email@example.com";
        var name = user.FindFirst("urn:google:name")?.Value ?? "User";
        var existingUser = await dbContext.Users.Find(u => u.Email == email).FirstOrDefaultAsync();

        if (existingUser == null)
        {
            var newUser = new User { Username = name, Email = email };
            await dbContext.Users.InsertOneAsync(newUser);
        }

        await context.Response.WriteAsync($"Hello, {name}! Your email is {email}.");
    }
    else
    {
        context.Response.Redirect("/login");
    }
});

app.Run();
