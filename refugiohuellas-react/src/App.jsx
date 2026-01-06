import { useEffect, useState } from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import { me } from "./api/api";

import Navbar from "./components/Navbar";
import Login from "./pages/Login";
import Dogs from "./pages/Dogs";
import DogDetail from "./pages/DogDetail";

import Profile from "./pages/Profile";


function PrivateRoute({ token, children }) {
    if (!token) return <Navigate to="/login" replace />;
    return children;
}

export default function App() {
    const [token, setToken] = useState(localStorage.getItem("token"));
    const [user, setUser] = useState(null);

    useEffect(() => {
        if (!token) {
            setUser(null);
            return;
        }

        me(token)
            .then(setUser)
            .catch(() => {
                localStorage.removeItem("token");
                setToken(null);
                setUser(null);
            });
    }, [token]);

    const logout = () => {
        localStorage.removeItem("token");
        setToken(null);
    };

    return (
        <div style={{ padding: 16 }}>
            <Navbar user={user} onLogout={logout} />

            <Routes>
                <Route
                    path="/login"
                    element={
                        token ? (
                            <Navigate to="/dogs" replace />
                        ) : (
                            <Login onLogin={setToken} />
                        )
                    }
                />

                <Route
                    path="/dogs"
                    element={
                        <PrivateRoute token={token}>
                            <Dogs />
                        </PrivateRoute>
                    }
                />

                <Route
                    path="/dogs/:id"
                    element={
                        <PrivateRoute token={token}>
                            <DogDetail token={token} />
                        </PrivateRoute>
                    }
                />

                <Route
                    path="/profile"
                    element={
                        <PrivateRoute token={token}>
                            <Profile token={token} />
                        </PrivateRoute>
                    }
                />



                <Route path="/" element={<Navigate to="/dogs" replace />} />
                <Route path="*" element={<Navigate to="/dogs" replace />} />
            </Routes>
        </div>
    );
}
