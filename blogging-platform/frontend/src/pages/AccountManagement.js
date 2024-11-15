// AccountManagement.js
import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import './AccountManagement.css'; // Import CSS file

function AccountManagement() {
    const [user, setUser] = useState(null);
    const navigate = useNavigate();

    useEffect(() => {
        axios.get('http://localhost:5042/profile', { withCredentials: true })
            .then(response => {
                const userData = response.data;
                const nameMatch = userData.match(/Hello, (.+?)!/);
                const emailMatch = userData.match(/Your email is (.+?)\./);
                const username = nameMatch ? nameMatch[1] : "Unknown User";
                const email = emailMatch ? emailMatch[1] : "no-email@example.com";
                setUser({ Username: username, Email: email });
            })
            .catch(error => console.error("Error fetching user profile:", error));
    }, []);

    const handleLogout = () => {
        axios.get('http://localhost:5042/logout', { withCredentials: true })
            .then(() => {
                setUser(null);
                navigate("/");
            })
            .catch(error => console.error("Error logging out:", error));
    };

    return (
        <div className="account-container">
            <div className="account-card">
                <h1>Account</h1>
                {user ? (
                    <>
                        <div className="profile-info">
                            <p><strong>Username:</strong> {user.Username}</p>
                            <p><strong>Email:</strong> {user.Email}</p>
                        </div>
                        <button onClick={handleLogout} className="logout-button">Logout</button>
                    </>
                ) : (
                    <div className="signin-section">
                        <h2>Login to Blogging Platform</h2>
                        <a href="http://localhost:5042/login" className="signin-button">Sign in with Google</a>
                    </div>
                )}
            </div>
        </div>
    );
}

export default AccountManagement;
