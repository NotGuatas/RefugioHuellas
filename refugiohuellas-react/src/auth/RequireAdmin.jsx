import { Navigate } from "react-router-dom";
import { useAuth } from "./AuthContext";

export default function RequireAdmin({ children }) {
  const { user, loading, isAuth } = useAuth();

  if (loading) return <div style={{ padding: 16 }}>Cargando...</div>;
  if (!isAuth) return <Navigate to="/login" replace />;

  const isAdmin =
    (user?.role || "").toLowerCase() === "admin" ||
    (Array.isArray(user?.roles) && user.roles.includes("Admin")) ||
    user?.isAdmin === true;

  return isAdmin ? children : <Navigate to="/dogs" replace />;
}
