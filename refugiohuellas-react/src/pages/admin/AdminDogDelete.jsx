import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import { adminApi } from "../../api";

export default function AdminDogDelete() {
  const { token } = useAuth();
  const { id } = useParams();
  const nav = useNavigate();

  const [dog, setDog] = useState(null);
  const [err, setErr] = useState("");

  useEffect(() => {
    async function load() {
      try {
        const d = await adminApi.dogGet(token, id);
        setDog(d);
      } catch (e) {
        setErr(e.message);
      }
    }
    load();
  }, [token, id]);

  async function del() {
    setErr("");
    try {
      await adminApi.dogDelete(token, id);
      nav("/admin/dogs");
    } catch (e) {
      setErr(e.message);
    }
  }

  return (
    <div style={{ display: "grid", gap: 12, maxWidth: 600 }}>
      <Link to="/admin/dogs">← Volver</Link>
      <h2 style={{ margin: 0 }}>Eliminar perro</h2>

      {err && <div style={{ color: "crimson" }}>{err}</div>}
      {!dog ? (
        <div>Cargando...</div>
      ) : (
        <>
          <div>
            ¿Seguro que deseas eliminar a <b>{dog.name}</b>?
          </div>
          <button onClick={del} style={{ background: "crimson", color: "white" }}>
            Sí, eliminar
          </button>
        </>
      )}
    </div>
  );
}
