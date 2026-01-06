import { useState } from "react";
import { login } from "../api/api";

export default function Login({ onLogin }) {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [err, setErr] = useState("");

    const handleSubmit = async (e) => {
        e.preventDefault();
        setErr("");
        try {
            const data = await login(email, password);
            localStorage.setItem("token", data.token);
            onLogin(data.token);
        } catch (ex) {
            setErr(ex.message);
        }
    };

    return (
        <div style={{ maxWidth: 360, margin: "40px auto" }}>
            <h2>Iniciar sesión</h2>

            {err && (
                <div style={{ background: "#ffd9d9", padding: 10, marginBottom: 10 }}>
                    {err}
                </div>
            )}

            <form onSubmit={handleSubmit}>
                <div style={{ marginBottom: 10 }}>
                    <label>Email</label>
                    <input
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        style={{ width: "100%", padding: 8 }}
                    />
                </div>

                <div style={{ marginBottom: 10 }}>
                    <label>Password</label>
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        style={{ width: "100%", padding: 8 }}
                    />
                </div>

                <button style={{ width: "100%", padding: 10 }}>
                    Entrar
                </button>
            </form>
        </div>
    );
}
