using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using MongoDB.Driver;
using MongoDB.Bson;
using DotNetEnv;



// Use environment variables for sensitive information and configurable settings
var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") ??
    "mongodb+srv://dbUser:15947150490@csc436cluster.wxaji.mongodb.net/BloggingPlatformDb?retryWrites=true&w=majority";
var databaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME") ?? "BloggingPlatformDb";
var frontendBaseUrl = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL") ?? "http://localhost:3000";
var googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
var googleClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
// var redirectUri = Environment.GetEnvironmentVariable("FRONTEND_ACCOUNT_PAGE") ?? $"{frontendBaseUrl}/account";
var redirectUri = "http://localhost:3000/account";



// Debugging logs
Console.WriteLine("Starting environment variable checks...");

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Error: MONGO_CONNECTION_STRING is missing.");
    Environment.Exit(1); // Exit the application
}
else
{
    Console.WriteLine($"MongoDB Connection String: {connectionString}");
}

if (string.IsNullOrEmpty(databaseName))
{
    Console.WriteLine("Error: MONGO_DATABASE_NAME is missing.");
    Environment.Exit(1); // Exit the application
}
else
{
    Console.WriteLine($"MongoDB Database Name: {databaseName}");
}

if (string.IsNullOrEmpty(frontendBaseUrl))
{
    Console.WriteLine("Error: FRONTEND_BASE_URL is missing.");
    Environment.Exit(1); // Exit the application
}
else
{
    Console.WriteLine($"Frontend Base URL: {frontendBaseUrl}");
}

if (string.IsNullOrEmpty(googleClientId))
{
    Console.WriteLine("Error: GOOGLE_CLIENT_ID is missing.");
    Environment.Exit(1); // Exit the application
}
else
{
    Console.WriteLine($"Google Client ID: {googleClientId}");
}

if (string.IsNullOrEmpty(googleClientSecret))
{
    Console.WriteLine("Error: GOOGLE_CLIENT_SECRET is missing.");
    Environment.Exit(1); // Exit the application
}
else
{
    Console.WriteLine($"Google Client Secret: (hidden for security)");
}

if (string.IsNullOrEmpty(redirectUri))
{
    Console.WriteLine("Error: FRONTEND_ACCOUNT_PAGE is missing.");
    Environment.Exit(1); // Exit the application
}
else
{
    Console.WriteLine($"Frontend Account Page Redirect URI: {redirectUri}");
}

Console.WriteLine("All environment variables are set!");



Console.WriteLine($"Connection String: {connectionString}");
Console.WriteLine($"Database Name: {databaseName}");
Console.WriteLine($"Frontend Base URL: {frontendBaseUrl}");
Console.WriteLine($"Google Client ID: {googleClientId}");
Console.WriteLine($"Redirect URI: {redirectUri}");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("MONGO_CONNECTION_STRING is not set.");
}
if (string.IsNullOrEmpty(googleClientId) || string.IsNullOrEmpty(googleClientSecret))
{
    throw new InvalidOperationException("Google OAuth credentials are not set.");
}

var client = new MongoClient(connectionString);
var database = client.GetDatabase(databaseName);

try
{
    // List collections as a simple check
    var collections = database.ListCollectionNames().ToList();
    Console.WriteLine($"Connected to MongoDB! Collections: {string.Join(", ", collections)}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error connecting to MongoDB: {ex.Message}");
    throw;
}

var builder = WebApplication.CreateBuilder(args);

// Configure CORS policy to allow requests from specific frontend URLs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", corsBuilder =>
    {
        corsBuilder.WithOrigins(
            "http://localhost:3000", // Local development
            "https://kind-water-06975370f.5.azurestaticapps.net" // Deployed frontend
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials(); // Allow cookies and credentials
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
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Google";
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.HttpOnly = true; // Ensure the cookie is not accessible via client-side JavaScript
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Use secure cookies in production
    options.Cookie.SameSite = SameSiteMode.None; // Allow cookies to work across different origins
    options.Cookie.Name = "BloggingPlatformAuth"; // Optional: Custom name for the cookie
    options.LoginPath = "/login"; // The endpoint for login
    options.LogoutPath = "/logout"; // The endpoint for logout
    options.AccessDeniedPath = "/access-denied"; // Optional: Path for unauthorized access
    options.ExpireTimeSpan = TimeSpan.FromDays(7); // Optional: Set cookie expiration
})
.AddGoogle("Google", options =>
{
    options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
    options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
    options.CallbackPath = "/signin-google"; // This must match the URI set in Google Console

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

// redirectUri = Environment.GetEnvironmentVariable("FRONTEND_ACCOUNT_PAGE") ?? $"{frontendBaseUrl}/account";

// Weather forecast example endpoint
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Define all API routes

app.MapGet("/", () => "Welcome to the backend of Blogging Platform!");

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

app.MapGet("/api/blogposts/{id}", async (MongoDbContext dbContext, string id) =>
{
    var blogPost = await dbContext.BlogPosts.Find(post => post.Id == id).FirstOrDefaultAsync();
    return blogPost is not null ? Results.Ok(blogPost) : Results.NotFound();
})
.WithName("GetBlogPostById");

app.MapGet("/api/blogposts", async (MongoDbContext dbContext) =>
{
    var blogPosts = await dbContext.BlogPosts.Find(_ => true).ToListAsync();
    return Results.Ok(blogPosts);
})
.WithName("GetAllBlogPosts");

app.MapGet("/api/user/blogposts", async (HttpContext context, MongoDbContext dbContext) =>
{
    var userEmail = context.User.FindFirst("urn:google:email")?.Value;

    if (string.IsNullOrEmpty(userEmail))
    {
        return Results.Unauthorized();
    }

    var user = await dbContext.Users.Find(u => u.Email == userEmail).FirstOrDefaultAsync();

    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    var userPosts = await dbContext.BlogPosts.Find(post => post.AuthorId == user.Id).ToListAsync();

    return Results.Ok(userPosts);
});
app.MapGet("/api/users", async (MongoDbContext dbContext) =>
{
    var users = await dbContext.Users.Find(_ => true).ToListAsync();
    return Results.Ok(users);
})
.WithName("GetAllUsers");

app.MapPost("/api/user/blogposts", async (HttpContext context, MongoDbContext dbContext, BlogPost newPost) =>
{
    var userEmail = context.User.FindFirst("urn:google:email")?.Value;

    if (string.IsNullOrEmpty(userEmail))
    {
        return Results.Unauthorized();
    }

    var user = await dbContext.Users.Find(u => u.Email == userEmail).FirstOrDefaultAsync();

    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    newPost.AuthorId = user.Id;

    await dbContext.BlogPosts.InsertOneAsync(newPost);

    return Results.Created($"/api/user/blogposts/{newPost.Id}", newPost);
});

app.MapPost("/api/blogposts/{id}/comments", async (HttpContext context, MongoDbContext dbContext, string id, Comment newComment) =>
{
    var userEmail = context.User.FindFirst("urn:google:email")?.Value;
    var userName = context.User.FindFirst("urn:google:name")?.Value;

    if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userName))
    {
        return Results.Unauthorized();
    }

    var user = await dbContext.Users.Find(u => u.Email == userEmail).FirstOrDefaultAsync();

    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    newComment.UserId = user.Id;
    newComment.Author = user.Username;
    newComment.Id = ObjectId.GenerateNewId().ToString();

    var update = Builders<BlogPost>.Update.Push("Comments", newComment);
    var result = await dbContext.BlogPosts.UpdateOneAsync(post => post.Id == id, update);

    return result.MatchedCount > 0 ? Results.Ok(newComment) : Results.NotFound();
});

app.MapPost("/api/blogposts", async (HttpContext context, MongoDbContext dbContext, BlogPost newPost) =>
{

    // Retrieve the authenticated user's email from Google Auth
    var userEmail = context.User.FindFirst("urn:google:email")?.Value;

    Console.WriteLine(context.User.Identity.IsAuthenticated); // Should be true
    Console.WriteLine(context.User.FindFirst("urn:google:email")?.Value); // Should print the user email


    if (string.IsNullOrEmpty(userEmail))
    {
        return Results.Unauthorized();
    }

    // Find the user's record in the database using their email
    var user = await dbContext.Users.Find(u => u.Email == userEmail).FirstOrDefaultAsync();

    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    // Set the AuthorId of the new post to the user's ID
    newPost.AuthorId = user.Id;

    // Insert the new post into the BlogPosts collection
    await dbContext.BlogPosts.InsertOneAsync(newPost);

    return Results.Created($"/api/blogposts/{newPost.Id}", newPost);
})
.WithName("CreateBlogPost");

app.MapPost("/api/users", async (MongoDbContext dbContext, User newUser) =>
{
    await dbContext.Users.InsertOneAsync(newUser);
    return Results.Created($"/api/users/{newUser.Id}", newUser);
});

app.MapPut("/api/blogposts/{id}", async (MongoDbContext dbContext, string id, BlogPost updatedPost) =>
{
    var result = await dbContext.BlogPosts.ReplaceOneAsync(post => post.Id == id, updatedPost);
    return result.MatchedCount > 0 ? Results.Ok(updatedPost) : Results.NotFound();
});

app.MapPut("/api/users/{id}", async (MongoDbContext dbContext, string id, User updatedUser) =>
{
    var result = await dbContext.Users.ReplaceOneAsync(user => user.Id == id, updatedUser);
    return result.MatchedCount > 0 ? Results.Ok(updatedUser) : Results.NotFound();
});

app.MapGet("/login", async context =>
{
    await context.ChallengeAsync("Google", new AuthenticationProperties
    {
        RedirectUri = redirectUri
    });
});

app.MapGet("/logout", async context =>
{
    await context.SignOutAsync("Cookies");
    context.Response.Redirect(frontendBaseUrl);
});

app.MapGet("/profile/setup", async context =>
{
    var htmlForm = @"
        <html>
        <body>
            <h2>Complete Your Profile</h2>
            <form method='post' action='/profile/setup'>
                <label for='name'>Name:</label><br>
                <input type='text' id='name' name='name' required><br><br>
                <input type='submit' value='Submit'>
            </form>
        </body>
        </html>";

    await context.Response.WriteAsync(htmlForm);
});

app.MapPost("/profile/setup", async (HttpContext context, MongoDbContext dbContext) =>
{
    var form = await context.Request.ReadFormAsync();
    var name = form["name"].ToString();

    var email = context.User.FindFirst("urn:google:email")?.Value;

    if (!string.IsNullOrEmpty(email))
    {
        var existingUser = await dbContext.Users.Find(u => u.Email == email).FirstOrDefaultAsync();

        if (existingUser == null)
        {
            var newUser = new User
            {
                Username = name,
                Email = email
            };
            await dbContext.Users.InsertOneAsync(newUser);
        }
        else
        {
            var update = Builders<User>.Update.Set(u => u.Username, name);
            await dbContext.Users.UpdateOneAsync(u => u.Email == email, update);
        }
    }

    context.Response.Redirect("/profile");
});

app.MapGet("/profile", async (HttpContext context, MongoDbContext dbContext) =>
{
    var user = context.User;

    if (user?.Identity?.IsAuthenticated ?? false)
    {
        var email = user.FindFirst("urn:google:email")?.Value ?? "no-email@example.com";
        var name = user.FindFirst("urn:google:name")?.Value;

        var existingUser = await dbContext.Users.Find(u => u.Email == email).FirstOrDefaultAsync();

        if (existingUser == null)
        {
            if (string.IsNullOrEmpty(name))
            {
                context.Response.Redirect("/profile/setup");
                return;
            }

            var newUser = new User
            {
                Username = name ?? "Unknown User",
                Email = email
            };
            await dbContext.Users.InsertOneAsync(newUser);
        }

        await context.Response.WriteAsync($"Hello, {name ?? "User"}! Your email is {email}.");
    }
    else
    {
        context.Response.Redirect("/login");
    }
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
