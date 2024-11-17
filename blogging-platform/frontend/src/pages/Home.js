// Home.js
import React, { useEffect, useState } from 'react';
import axios from 'axios';
import BlogPosts from '../components/BlogPosts';
import SubmitPost from '../components/SubmitPost';
import './Home.css';


const API_BASE_URL = process.env.REACT_APP_API_BASE_URL;

// const API_BASE_URL = "https://my-backend-app-debne7hgd7gjgvd5.canadacentral-01.azurewebsites.net";
// const API_BASE_URL = "http://localhost:5042";

const Home = () => {
    const [isSignedIn, setIsSignedIn] = useState(false);
    const [userName, setUserName] = useState('');

    useEffect(() => {
        axios.get(`${API_BASE_URL}/profile`, { withCredentials: true })
            .then(response => {
                setIsSignedIn(true);

                // Extract user name from response
                const userData = response.data;
                const nameMatch = userData.match(/Hello, (.+?)!/);
                const username = nameMatch ? nameMatch[1] : "User";
                setUserName(username);
            })
            .catch(error => {
                setIsSignedIn(false);
            });
    }, []);

    return (
        <div className="home-container">
            <h1 className="title">{userName}'s Blogging Platform</h1>
            {isSignedIn ? (
                <div className="block-container">
                    <div className="block block-left">
                        <BlogPosts /> {/* Block 1 */}
                    </div>
                    <div className="block block-right">
                        <SubmitPost /> {/* Block 2 */}
                    </div>
                </div>
            ) : (
                <div className="sign-in-message">
                    <p>Please sign in first to submit a post.</p>
                </div>
            )}
        </div>
    );
};

export default Home;
