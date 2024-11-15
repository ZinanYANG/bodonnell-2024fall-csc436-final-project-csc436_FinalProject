import React from 'react';
import { BrowserRouter as Router, Route, Routes, Link } from 'react-router-dom';
import Home from './pages/Home';
import PostDetail from './pages/PostDetail';
import AccountManagement from './pages/AccountManagement';
import './styles.css';

function App() {
  return (
    <Router>
      <nav>
        <Link to="/">Home</Link> | <Link to="/account">Account</Link>
      </nav>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/post/:id" element={<PostDetail />} />
        <Route path="/account" element={<AccountManagement />} />
      </Routes>
    </Router>
  );
}

export default App;
