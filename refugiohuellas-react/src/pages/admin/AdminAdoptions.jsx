import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import { adminApi } from "../../api";

export default function AdminAdoptions() {
  const { token } = useAuth();
  const [items, setItems] = useState([]);
  const [err, setErr] = useState("");

  useEffect(() => {
    async function load() {
      try {
        const data = await adminApi.adoptionsList(token);
        setItems(data);
      } catch (e) {
        setErr(e.message);
      }
    }
    load();
  }, [token]);

  return (
    <div style={{ display: "grid", gap: 12 }}>
      <h2 style={{ margin: 0 }}>Admin: Solicitudes</h2>
      {err && <div style={{ color: "crimson" }}>{err}</div>}

      <div style={{ overflowX: "auto" }}>
        <table width="100%" cellPadding="8" style={{ borderCollapse: "collapse" }}>
          <thead>
            <tr style={{ textAlign: "left", borderBottom: "1px solid #ddd" }}>
              <th>ID</th>
              <th>Usuario</th>
              <th>Perro</th>
              <th>Compat</th>
              <th>Estado</th>
              <th>Fecha</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {items.map((a) => (
              <tr key={a.id} style={{ borderBottom: "1px solid #f0f0f0" }}>
                <td>{a.id}</td>
                <td>{a.userEmail || a.userId}</td>
                <td>{a.dogName}</td>
                <td>{a.compatibilityScore}%</td>
                <td>{a.status}</td>
                <td>{new Date(a.createdAt).toLocaleString()}</td>
                <td>
                  <Link to={`/admin/adoptions/${a.id}`}>Ver</Link>
                </td>
              </tr>
            ))}
            {!items.length && (
              <tr>
                <td colSpan="7" style={{ padding: 16, color: "#666" }}>
                  No hay solicitudes.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
