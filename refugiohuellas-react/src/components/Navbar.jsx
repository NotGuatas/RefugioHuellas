import { Link } from "react-router-dom";

export default function Navbar({ user, onLogout }) {
    return (
        <div
            style={{
                display: "flex",
                gap: 12,
                alignItems: "center",
                justifyContent: "space-between",
                padding: 12,
                border: "1px solid #ddd",
                borderRadius: 12,
                marginBottom: 16,
            }}
        >
            <div style={{ display: "flex", gap: 12, alignItems: "center" }}>
                <Link to="/dogs" style={{ textDecoration: "none", fontWeight: 700 }}>
                    RefugioHuellas
                </Link>
                {user && (
                    <span style={{ fontSize: 14, color: "#444" }}>
                        {user.email} {user.roles?.length ? `(${user.roles.join(", ")})` : ""}
                    </span>
                )}
            </div>

            <div style={{ display: "flex", gap: 10 }}>
                {!user ? (
                    <Link to="/login">Login</Link>
                ) : (
                    <button onClick={onLogout}>Cerrar sesi�n</button>
                )}
            </div>
        </div>
    );
}
