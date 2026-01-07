import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "./AuthContext";

export default function RequireAuth({ children }) {
    const { isAuth, loading } = useAuth();
    const loc = useLocation();

    if (loading) return null;
    if (!isAuth) return <Navigate to="/login" replace state={{ from: loc }} />;
    return children;
}
