import { Routes, Route, Navigate } from "react-router-dom";
import Navbar from "./components/Navbar";

import Login from "./pages/Login";
import Dogs from "./pages/Dogs";
import DogDetail from "./pages/DogDetail";
import Profile from "./pages/Profile";

import RequireAuth from "./auth/RequireAuth";
import { useAuth } from "./auth/AuthContext";

export default function App() {
    const { user, token, logout, isAuth, loading } = useAuth();

    if (loading) {
        return <div style={{ padding: 16 }}>Cargando...</div>;
    }

    return (
        <div style={{ padding: 16 }}>
            <Navbar user={user} onLogout={logout} />

            <Routes>
                <Route
                    path="/login"
                    element={isAuth ? <Navigate to="/dogs" replace /> : <Login />}
                />

                <Route
                    path="/dogs"
                    element={
                        <RequireAuth>
                            <Dogs />
                        </RequireAuth>
                    }
                />

                <Route
                    path="/dogs/:id"
                    element={
                        <RequireAuth>
                            <DogDetail token={token} />
                        </RequireAuth>
                    }
                />

                <Route
                    path="/profile"
                    element={
                        <RequireAuth>
                            <Profile token={token} />
                        </RequireAuth>
                    }
                />

                <Route path="/" element={<Navigate to={isAuth ? "/dogs" : "/login"} replace />} />
                <Route path="*" element={<Navigate to={isAuth ? "/dogs" : "/login"} replace />} />
            </Routes>
        </div>
    );
}
