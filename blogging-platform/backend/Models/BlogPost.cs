using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

public class BlogPost
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [BsonElement("comments")]
    public List<Comment> Comments { get; set; } = new List<Comment>();

    // New field to tie the blog post to a user
    [BsonElement("authorId")]
    public string AuthorId { get; set; } = string.Empty; // ID of the user who authored the post
}


