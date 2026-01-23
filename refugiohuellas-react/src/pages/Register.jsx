import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { authApi } from "../api";
import { useAuth } from "../auth/AuthContext";

export default function Register() {
  const nav = useNavigate();
  const { login } = useAuth();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [err, setErr] = useState("");
  const [ok, setOk] = useState("");

  const submit = async (e) => {
    e.preventDefault();
    setErr("");
    setOk("");

    if (password !== confirm) {
      setErr("Las contraseñas no coinciden.");
      return;
    }

    try {
      await authApi.register(email, password);
      setOk("Cuenta creada. Iniciando sesión...");

      await login(email, password);

      nav("/app/dogs"); // ✅ coherente con tu base "/app"
    } catch (ex) {
      setErr(ex.message || "Error");
    }
  };

  return (
    <div style={{ maxWidth: 420, margin: "40px auto" }}>
      <h2>Crear cuenta</h2>

      {err && <div style={{ color: "crimson", marginBottom: 10 }}>{err}</div>}
      {ok && <div style={{ color: "green", marginBottom: 10 }}>{ok}</div>}

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

        <div>
          <label>Confirmar password</label>
          <input
            type="password"
            value={confirm}
            onChange={(e) => setConfirm(e.target.value)}
            style={{ width: "100%", padding: 8 }}
          />
        </div>

        <button style={{ width: "100%", padding: 10 }}>Crear cuenta</button>
      </form>

      <div style={{ marginTop: 12 }}>
        ¿Ya tienes cuenta? <Link to="/app/login">Inicia sesión</Link>
      </div>
    </div>
  );
}
