import React, { useState } from 'react';
import axios from 'axios';

function SubmitPost() {
    const [title, setTitle] = useState('');
    const [content, setContent] = useState('');

    const handleSubmit = (e) => {
        e.preventDefault();
        axios.post('http://localhost:5042/api/user/blogposts', { title, content }, { withCredentials: true })
            .then(() => {
                setTitle('');
                setContent('');
                alert('Post submitted successfully!');
                window.location.reload(); // Refresh to show the new post on the home page
            })
            .catch(error => console.error("Error submitting post:", error));
    };

    return (
        <form onSubmit={handleSubmit}>
            <h2>Create a New Post</h2>
            <div>
                <label>Title:</label>
                <input
                    type="text"
                    value={title}
                    onChange={(e) => setTitle(e.target.value)}
                    required
                />
            </div>
            <div>
                <label>Content:</label>
                <textarea
                    value={content}
                    onChange={(e) => setContent(e.target.value)}
                    required
                ></textarea>
            </div>
            <button type="submit">Submit Post</button>
        </form>
    );
}

export default SubmitPost;
