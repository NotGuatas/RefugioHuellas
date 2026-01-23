import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export default function Login() {
  const { login } = useAuth();
  const nav = useNavigate();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [err, setErr] = useState("");

  const submit = async (e) => {
    e.preventDefault();
    setErr("");
    try {
      await login(email, password);
      nav("/app/dogs"); // ✅ coherente con tu base "/app"
    } catch (ex) {
      setErr(ex.message || "Error");
    }
  };

  return (
    <div style={{ maxWidth: 420, margin: "40px auto" }}>
      <h2>Iniciar sesión</h2>

      {err && <div style={{ color: "crimson", marginBottom: 10 }}>{err}</div>}

      <form onSubmit={submit} style={{ display: "grid", gap: 10 }}>
        <div>
          <label>Email</label>
          <input
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            style={{ width: "100%", padding: 8 }}
          />
        </div>

        <div>
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

      <div style={{ marginTop: 12 }}>
        ¿No tienes cuenta? <Link to="/register">Crear cuenta</Link>
      </div>
    </div>
  );
}
