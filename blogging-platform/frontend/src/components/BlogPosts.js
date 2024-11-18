// BlogPosts.js
import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { Link } from 'react-router-dom';

const API_BASE_URL = process.env.REACT_APP_API_BASE_URL;

const BlogPosts = () => {
    const [posts, setPosts] = useState([]);
    const [error, setError] = useState(null);

    useEffect(() => {
        console.log("API_BASE_URL:", API_BASE_URL);

        axios.get(`${API_BASE_URL}/api/user/blogposts`, { withCredentials: true })
            .then(response => setPosts(response.data))
            .catch(error => {
                console.error("Error fetching posts:", error);
                setError("Failed to fetch posts. Please try again.");
            });
    }, []);

    return (
        <div>
            <h2>Your Blog Posts</h2>
            {error && <p className="error">{error}</p>}
            {posts.length === 0 ? (
                <p>No posts available.</p>
            ) : (
                <ul>
                    {posts.map(post => (
                        <li key={post?.id}>
                            <Link to={`/post/${post?.id}`}>
                                <h3>{post?.title}</h3>
                            </Link>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
};

export default BlogPosts;
