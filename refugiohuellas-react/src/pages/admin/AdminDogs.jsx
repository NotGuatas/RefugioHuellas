import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import { adminApi } from "../../api";

export default function AdminDogs() {
  const { token } = useAuth();

  const [dogs, setDogs] = useState([]);
  const [err, setErr] = useState("");

  const load = useCallback(async () => {
    setErr("");
    try {
      const data = await adminApi.dogsList(token);
      setDogs(data);
    } catch (e) {
      setErr(e.message);
    }
  }, [token]);

  useEffect(() => {
    load();
  }, [load]);

  return (
    <div style={{ display: "grid", gap: 12 }}>
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <h2 style={{ margin: 0 }}>Admin: Perros</h2>
        <Link to="/admin/dogs/new">+ Nuevo perro</Link>
      </div>

      {err && <div style={{ color: "crimson" }}>{err}</div>}

      <table width="100%" cellPadding="8" style={{ borderCollapse: "collapse" }}>
        <thead>
          <tr style={{ textAlign: "left", borderBottom: "1px solid #ddd" }}>
            <th>ID</th>
            <th>Foto</th>
            <th>Nombre</th>
            <th>Raza</th>
            <th>Tamaño</th>
            <th>Energía</th>
            <th>Origen</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          {dogs.map((d) => (
            <tr key={d.id} style={{ borderBottom: "1px solid #f0f0f0" }}>
              <td>{d.id}</td>
              <td>
                <img
                  src={d.photoUrl || "https://via.placeholder.com/80x50"}
                  alt={d.name}
                  style={{
                    width: 80,
                    height: 50,
                    objectFit: "cover",
                    borderRadius: 8,
                  }}
                />
              </td>
              <td>{d.name}</td>
              <td>{d.breed}</td>
              <td>{d.size}</td>
              <td>{d.energyLevel}</td>
              <td>{d.originTypeName || "-"}</td>
              <td>
                <div style={{ display: "flex", gap: 8 }}>
                  <Link to={`/admin/dogs/${d.id}/edit`}>Editar</Link>
                  <Link
                    to={`/admin/dogs/${d.id}/delete`}
                    style={{ color: "crimson" }}
                  >
                    Eliminar
                  </Link>
                </div>
              </td>
            </tr>
          ))}

          {!dogs.length && (
            <tr>
              <td colSpan="8" style={{ padding: 16, color: "#666" }}>
                No hay perros registrados.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
