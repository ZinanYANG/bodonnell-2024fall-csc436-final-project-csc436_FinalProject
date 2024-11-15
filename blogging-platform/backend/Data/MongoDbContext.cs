using MongoDB.Driver;

/*
class creates connections to 
the Users 
BlogPosts
and Comments collections in MongoDB
*/
public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    public IMongoCollection<BlogPost> BlogPosts => _database.GetCollection<BlogPost>("BlogPosts");
    public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("Comments");
}
