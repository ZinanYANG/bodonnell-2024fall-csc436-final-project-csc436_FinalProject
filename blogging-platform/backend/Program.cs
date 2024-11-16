using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using MongoDB.Driver;
using MongoDB.Bson;

var connectionString = "mongodb+srv://dbUser:15947150490@csc436cluster.wxaji.mongodb.net/BloggingPlatformDb?retryWrites=true&w=majority";
var databaseName = "BloggingPlatformDb";

var builder = WebApplication.CreateBuilder(args);

// Configure CORS policy to allow requests from the frontend
// Add this CORS policy configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
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
builder.Services.AddAuthorization();  // This line resolves the error

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

<<<<<<< HEAD
// Weather forecast example endpoint
=======
// Example API endpoint
>>>>>>> 24ccffa1c7a17d2fe3a0927d4c95ca985b9d1093
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

<<<<<<< HEAD

=======
>>>>>>> 24ccffa1c7a17d2fe3a0927d4c95ca985b9d1093
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

<<<<<<< HEAD

/*
Fetch an Individual Blog Post by ID
http://localhost:5042/api/blogposts/{id}
replace {id} with the actual blog ID
*/
=======
// Replace all localhost URLs with your actual deployed URLs
>>>>>>> 24ccffa1c7a17d2fe3a0927d4c95ca985b9d1093
app.MapGet("/api/blogposts/{id}", async (MongoDbContext dbContext, string id) =>
{
    var blogPost = await dbContext.BlogPosts.Find(post => post.Id == id).FirstOrDefaultAsync();
    return blogPost is not null ? Results.Ok(blogPost) : Results.NotFound();
})
.WithName("GetBlogPostById");

app.MapGet("/api/user/blogposts", async (HttpContext context, MongoDbContext dbContext) =>
{
<<<<<<< HEAD
    // Get the current user's email from the authentication context
    var userEmail = context.User.FindFirst("urn:google:email")?.Value;

    if (string.IsNullOrEmpty(userEmail))
    {
        return Results.Unauthorized();
    }

    // Find the user in the database by email to get their Id
    var user = await dbContext.Users.Find(u => u.Email == userEmail).FirstOrDefaultAsync();

    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    // Use the user's Id as the AuthorId to fetch their posts
    var userPosts = await dbContext.BlogPosts.Find(post => post.AuthorId == user.Id).ToListAsync();

    return Results.Ok(userPosts);
});


app.MapPost("/api/user/blogposts", async (HttpContext context, MongoDbContext dbContext, BlogPost newPost) =>
{
    // Get the current user's email from the authentication context
    var userEmail = context.User.FindFirst("urn:google:email")?.Value;

    if (string.IsNullOrEmpty(userEmail))
    {
        return Results.Unauthorized();
    }

    // Find the user in the database by email to get their Id
    var user = await dbContext.Users.Find(u => u.Email == userEmail).FirstOrDefaultAsync();

    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    // Assign the user's Id as the authorId of the blog post
    newPost.AuthorId = user.Id;

    // Insert the new post into the BlogPosts collection
    await dbContext.BlogPosts.InsertOneAsync(newPost);

    return Results.Created($"/api/user/blogposts/{newPost.Id}", newPost);
});



// MongoDB API endpoint to get all blog posts
/* 
Fetch All Blog Posts 
http://localhost:5042/api/blogposts
*/
=======
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

>>>>>>> 24ccffa1c7a17d2fe3a0927d4c95ca985b9d1093
app.MapGet("/api/blogposts", async (MongoDbContext dbContext) =>
{
    var blogPosts = await dbContext.BlogPosts.Find(_ => true).ToListAsync();
    return Results.Ok(blogPosts);
})
.WithName("GetAllBlogPosts");

<<<<<<< HEAD
app.MapGet("/api/user/posts", async (HttpContext context, MongoDbContext dbContext) =>
{
    // Get the current user's email from the authentication context
    var userEmail = context.User.FindFirst("urn:google:email")?.Value;

    if (string.IsNullOrEmpty(userEmail))
    {
        return Results.Unauthorized();
    }

    // Find the user in the database by email to get their Id
    var user = await dbContext.Users.Find(u => u.Email == userEmail).FirstOrDefaultAsync();

    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    // Use the user's Id as the AuthorId to fetch their posts
    var userPosts = await dbContext.BlogPosts.Find(post => post.AuthorId == user.Id).ToListAsync();

    return Results.Ok(userPosts);
});

// Define login endpoint
=======
>>>>>>> 24ccffa1c7a17d2fe3a0927d4c95ca985b9d1093
app.MapGet("/login", async context =>
{
    await context.ChallengeAsync("Google", new AuthenticationProperties
    {
<<<<<<< HEAD
        // After successful login, Google will redirect to /profile
        // RedirectUri = "/profile"  
        RedirectUri = "http://localhost:3000/account"  // Redirect to frontend account page


=======
        RedirectUri = "https://nice-moss-014326b10.5.azurestaticapps.net/account"
>>>>>>> 24ccffa1c7a17d2fe3a0927d4c95ca985b9d1093
    });
});

app.MapGet("/api/users", async (MongoDbContext dbContext) =>
{
    // Fetch all users from the Users collection
    var users = await dbContext.Users.Find(_ => true).ToListAsync();
    return Results.Ok(users);
})
.WithName("GetAllUsers");


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

    // Retrieve user email from the authenticated context
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
            // Update the user’s name if they already exist
            var update = Builders<User>.Update.Set(u => u.Username, name);
            await dbContext.Users.UpdateOneAsync(u => u.Email == email, update);
        }
    }

    context.Response.Redirect("/profile");
});


// Define profile endpoint
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
            // Redirect to profile setup if name is missing
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



app.MapPost("/api/blogposts", async (HttpContext context, MongoDbContext dbContext, BlogPost newPost) =>
{
    // Retrieve the authenticated user's email from Google Auth
    var userEmail = context.User.FindFirst("urn:google:email")?.Value;

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


/* Add a Comment to a Blog Post 
http://localhost:5042/api/blogposts/{id}/comments 
(replace {id} with the actual blog post ID).
*/
app.MapPost("/api/blogposts/{id}/comments", async (HttpContext context, MongoDbContext dbContext, string id, Comment newComment) =>
{
    // Get the logged-in user's email and username from the context
    var userEmail = context.User.FindFirst("urn:google:email")?.Value;
    var userName = context.User.FindFirst("urn:google:name")?.Value;

    if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userName))
    {
        return Results.Unauthorized();
    }

    // Find the user in the database by email to get their Id
    var user = await dbContext.Users.Find(u => u.Email == userEmail).FirstOrDefaultAsync();

    if (user == null)
    {
        return Results.NotFound("User not found.");
    }


    // Set the userId and author of the new comment to the authenticated user
    newComment.UserId = user.Id;
    newComment.Author = user.Username;
    newComment.Id = ObjectId.GenerateNewId().ToString(); // Generate a unique ID for the comment

    // Update the BlogPost to add the new comment
    var update = Builders<BlogPost>.Update.Push("Comments", newComment);
    var result = await dbContext.BlogPosts.UpdateOneAsync(post => post.Id == id, update);

    return result.MatchedCount > 0 ? Results.Ok(newComment) : Results.NotFound();
})
.WithName("AddCommentToBlogPost");




/*
Add a Users Collection in MongoDB
*/
app.MapPost("/api/users", async (MongoDbContext dbContext, User newUser) =>
{
    await dbContext.Users.InsertOneAsync(newUser);
    return Results.Created($"/api/users/{newUser.Id}", newUser);
})
.WithName("CreateUserAccount");



/*

Update an Existing Blog Post

*/
app.MapPut("/api/blogposts/{id}", async (MongoDbContext dbContext, string id, BlogPost updatedPost) =>
{
    var result = await dbContext.BlogPosts.ReplaceOneAsync(post => post.Id == id, updatedPost);
    return result.MatchedCount > 0 ? Results.Ok(updatedPost) : Results.NotFound();
})
.WithName("UpdateBlogPost");

app.MapPut("/api/users/{id}", async (MongoDbContext dbContext, string id, User updatedUser) =>
{
    var result = await dbContext.Users.ReplaceOneAsync(user => user.Id == id, updatedUser);
    return result.MatchedCount > 0 ? Results.Ok(updatedUser) : Results.NotFound();
})
.WithName("UpdateUserDetails");

app.MapGet("/logout", async context =>
{
    await context.SignOutAsync("Cookies");
<<<<<<< HEAD
    context.Response.Redirect("http://localhost:3000"); // Redirect to the homepage or login page on the frontend
}).WithName("Logout");




=======
    context.Response.Redirect("https://nice-moss-014326b10.5.azurestaticapps.net"); // Redirect to the homepage or login page on the frontend
}).WithName("Logout");

>>>>>>> 24ccffa1c7a17d2fe3a0927d4c95ca985b9d1093
app.Run();

// Record for WeatherForecast, used by the example weather endpoint
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
