// PostDetail.js
import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import axios from 'axios';
import CommentForm from '../components/CommentForm';
import './PostDetail.css';

function PostDetail() {
    const { id } = useParams();
    const [post, setPost] = useState(null);
    const [isEditing, setIsEditing] = useState(false);
    const [editTitle, setEditTitle] = useState('');
    const [editContent, setEditContent] = useState('');

    useEffect(() => {
        axios.get(`http://localhost:5042/api/blogposts/${id}`)
            .then(response => {
                setPost(response.data);
                setEditTitle(response.data.title);
                setEditContent(response.data.content);
            })
            .catch(error => console.error("Error fetching post:", error));
    }, [id]);

    const handleEditClick = () => {
        setIsEditing(true);
    };

    const handleSaveClick = () => {
        axios.put(`http://localhost:5042/api/blogposts/${id}`, {
            ...post,
            title: editTitle,
            content: editContent
        }, { withCredentials: true })
            .then(response => {
                setPost(response.data);
                setIsEditing(false);
            })
            .catch(error => console.error("Error updating post:", error));
    };

    return post ? (
        <div className="post-detail-container">
            {/* Block 3: Post Details */}
            <div className="post-details">
                {isEditing ? (
                    <div className="edit-post-form">
                        <input
                            type="text"
                            value={editTitle}
                            onChange={(e) => setEditTitle(e.target.value)}
                            placeholder="Edit Title"
                        />
                        <textarea
                            value={editContent}
                            onChange={(e) => setEditContent(e.target.value)}
                            placeholder="Edit Content"
                        />
                        <button onClick={handleSaveClick}>Save Changes</button>
                    </div>
                ) : (
                    <>
                        <h1>{post.title}</h1>
                        <p>{post.content}</p>
                        <button onClick={handleEditClick}>Edit Post</button>
                    </>
                )}
                <p><strong>Author ID:</strong> {post.authorId}</p>
                <p><strong>Created At:</strong> {new Date(post.createdAt).toLocaleString()}</p>
            </div>

            {/* Block 4: Comments Section */}
            <div className="comments-section">
                <h3>Comments</h3>
                {post.comments && post.comments.length > 0 ? (
                    <ul>
                        {post.comments.map((comment) => (
                            <li key={comment.id} className="comment">
                                <p><strong>Author:</strong> {comment.author}</p>
                                <p>{comment.content}</p>
                                <p><em>Posted on: {new Date(comment.postedAt).toLocaleString()}</em></p>
                            </li>
                        ))}
                    </ul>
                ) : (
                    <p>No comments available.</p>
                )}
                <CommentForm postId={id} />
            </div>
        </div>
    ) : <p>Loading...</p>;
}

export default PostDetail;
