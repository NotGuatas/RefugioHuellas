import { useCallback, useEffect, useState } from "react";
import { useAuth } from "../../auth/AuthContext";
import { adminApi } from "../../api";

export default function AdminTopMatches() {
  const { token } = useAuth();

  const [data, setData] = useState(null);
  const [err, setErr] = useState("");
  const [ok, setOk] = useState("");

  const load = useCallback(async () => {
    setErr("");
    setOk("");
    try {
      // MVC usa la ventana default (7). Aquí lo cargamos fijo.
      const res = await adminApi.matchesTop(token, 7);
      setData(res);
    } catch (e) {
      setErr(e.message);
    }
  }, [token]);

  useEffect(() => {
    load();
  }, [load]);

  async function approve(applicationId) {
    setErr("");
    setOk("");
    try {
      const res = await adminApi.matchApprove(token, applicationId);
      setOk(res.message);
      await load();
    } catch (e) {
      setErr(e.message);
    }
  }

  return (
    <div style={{ display: "grid", gap: 12 }}>
      <h2 style={{ margin: 0 }}>Admin: Top Matches</h2>

      {err && <div style={{ color: "crimson" }}>{err}</div>}
      {ok && <div style={{ color: "green" }}>{ok}</div>}

      {!data ? (
        <div>Cargando...</div>
      ) : (
        <div style={{ display: "grid", gap: 12 }}>
          {data.dogs.map((d) => (
            <div
              key={d.dogId}
              style={{ border: "1px solid #ddd", borderRadius: 12, padding: 12 }}
            >
              <div style={{ display: "flex", gap: 12, alignItems: "center" }}>
                <img
                  src={d.dogPhotoUrl || "https://via.placeholder.com/100x70"}
                  alt={d.dogName}
                  style={{
                    width: 100,
                    height: 70,
                    objectFit: "cover",
                    borderRadius: 10,
                  }}
                />
                <div>
                  <div style={{ fontWeight: 700 }}>{d.dogName}</div>
                  <div style={{ color: "#666" }}>
                    Ingreso: {new Date(d.dogIntakeDate).toLocaleString()}
                  </div>
                </div>
              </div>

              <table
                width="100%"
                cellPadding="8"
                style={{ borderCollapse: "collapse", marginTop: 10 }}
              >
                <thead>
                  <tr style={{ textAlign: "left", borderBottom: "1px solid #eee" }}>
                    <th>Usuario</th>
                    <th>Compatibilidad</th>
                    <th>Estado</th>
                    <th>Fecha</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {d.topCandidates.map((c) => (
                    <tr key={c.applicationId} style={{ borderBottom: "1px solid #f3f3f3" }}>
                      <td>{c.userEmail}</td>
                      <td><b>{c.compatibilityScore}%</b></td>
                      <td>{c.status}</td>
                      <td>{new Date(c.createdAt).toLocaleString()}</td>
                      <td>
                        <button
                          onClick={() => approve(c.applicationId)}
                          disabled={c.status === "Aprobada"}
                        >
                          {c.status === "Aprobada" ? "Aprobada" : "Aprobar"}
                        </button>
                      </td>
                    </tr>
                  ))}
                  {!d.topCandidates.length && (
                    <tr>
                      <td colSpan="5" style={{ color: "#666", padding: 12 }}>
                        (Sin solicitudes en la ventana)
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          ))}

          {!data.dogs.length && (
            <div style={{ color: "#666" }}>No hay perros en la ventana.</div>
          )}
        </div>
      )}
    </div>
  );
}
