// BlogPosts.js
import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { Link } from 'react-router-dom';

const API_BASE_URL = process.env.REACT_APP_API_BASE_URL;

// const API_BASE_URL = "https://my-backend-app-debne7hgd7gjgvd5.canadacentral-01.azurewebsites.net";
// const API_BASE_URL = "http://localhost:5042";

const BlogPosts = () => {
    const [posts, setPosts] = useState([]);

    useEffect(() => {
        // Use the endpoint to fetch posts for the logged-in user
        axios.get(`${API_BASE_URL}/api/user/blogposts`, { withCredentials: true })
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
