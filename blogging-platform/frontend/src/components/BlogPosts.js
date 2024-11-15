// BlogPosts.js
import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { Link } from 'react-router-dom';

const BlogPosts = () => {
    const [posts, setPosts] = useState([]);

    useEffect(() => {
        // Use the endpoint to fetch posts for the logged-in user
        axios.get('http://localhost:5042/api/user/blogposts', { withCredentials: true })
            .then(response => setPosts(response.data))
            .catch(error => console.error("Error fetching posts:", error));
    }, []);

    return (
        <div>
            <h2>Your Blog Posts</h2>
            {posts.length === 0 ? (
                <p>No posts available.</p>
            ) : (
                <ul>
                    {posts.map(post => (
                        <li key={post.id}>
                            <Link to={`/post/${post.id}`}>
                                <h3>{post.title}</h3>
                            </Link>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
};

export default BlogPosts;
