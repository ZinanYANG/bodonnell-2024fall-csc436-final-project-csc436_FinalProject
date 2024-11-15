import React, { useState } from 'react';
import axios from 'axios';

function CommentForm({ postId }) {
    const [content, setContent] = useState('');

    const handleSubmit = (e) => {
        e.preventDefault();
        axios.post(`http://localhost:5042/api/blogposts/${postId}/comments`, { content }, { withCredentials: true })
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
