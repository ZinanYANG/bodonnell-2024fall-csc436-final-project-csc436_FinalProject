import React, { useState } from 'react';
import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_BASE_URL;

// const API_BASE_URL = "https://my-backend-app-debne7hgd7gjgvd5.canadacentral-01.azurewebsites.net";
// const API_BASE_URL = "http://localhost:5042";

function CommentForm({ postId }) {
    const [content, setContent] = useState('');

    const handleSubmit = (e) => {
        e.preventDefault();
        axios.post(`${API_BASE_URL}/api/blogposts/${postId}/comments`, { content }, { withCredentials: true })
            .then(() => {
                setContent('');
                alert('Comment added!');
                window.location.reload(); // Reload to show new comment
            })
            .catch(error => console.error(error));
    };

    return (
        <form onSubmit={handleSubmit}>
            <textarea
                value={content}
                onChange={(e) => setContent(e.target.value)}
                placeholder="Add a comment..."
                required
            />
            <button type="submit">Submit</button>
        </form>
    );
}

export default CommentForm;
