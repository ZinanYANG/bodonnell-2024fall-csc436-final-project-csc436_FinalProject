using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

public class Comment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfDefault] // This ensures MongoDB generates an Id if it's null or default
    public string? Id { get; set; }

    public string UserId { get; set; } = string.Empty; // ID of the user who posted the comment
    public string Author { get; set; } = string.Empty; // User's display name or username
    public string Content { get; set; } = string.Empty;
    public DateTime PostedAt { get; set; } = DateTime.Now;
}
