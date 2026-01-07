import { useState } from "react";
import { useAuth } from "../auth/AuthContext";

export default function Login() {
    const { login } = useAuth();
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [err, setErr] = useState("");

    const submit = async (e) => {
        e.preventDefault();
        setErr("");
        try {
            await login(email, password);
        } catch (ex) {
            setErr(ex.message || "Error");
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

            <form onSubmit={submit}>
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

                <button style={{ width: "100%", padding: 10 }}>Entrar</button>
            </form>
        </div>
    );
}
