import { useCallback, useEffect, useState } from "react";
import { useAuth } from "../../auth/AuthContext";
import { adminApi } from "../../api";

export default function AdminBestMatches() {
  const { token } = useAuth();

  const [items, setItems] = useState([]);
  const [err, setErr] = useState("");
  const [ok, setOk] = useState("");

  const load = useCallback(async () => {
    setErr("");
    setOk("");
    try {
      const data = await adminApi.matchesBest(token);
      setItems(data);
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
      <h2 style={{ margin: 0 }}>Admin: Best Matches</h2>

      {err && <div style={{ color: "crimson" }}>{err}</div>}
      {ok && <div style={{ color: "green" }}>{ok}</div>}

      <div style={{ display: "grid", gap: 10 }}>
        {items.map((m) => (
          <div
            key={m.dogId}
            style={{
              border: "1px solid #ddd",
              borderRadius: 12,
              padding: 12,
              display: "flex",
              gap: 12,
              alignItems: "center",
            }}
          >
            <img
              src={m.dogPhotoUrl || "https://via.placeholder.com/100x70"}
              alt={m.dogName}
              style={{
                width: 100,
                height: 70,
                objectFit: "cover",
                borderRadius: 10,
              }}
            />

            <div style={{ flex: 1 }}>
              <div style={{ fontWeight: 700 }}>{m.dogName}</div>
              <div style={{ color: "#666" }}>
                Ingreso: {new Date(m.dogIntakeDate).toLocaleDateString()}
              </div>

              {m.applicationId ? (
                <>
                  <div>
                    Ganador: <b>{m.userEmail}</b>
                  </div>
                  <div>
                    Compat: <b>{m.compatibilityScore}%</b> — Estado:{" "}
                    <b>{m.status}</b>
                  </div>
                </>
              ) : (
                <div style={{ color: "#666" }}>Sin solicitudes</div>
              )}
            </div>

            {m.applicationId && (
              <button
                onClick={() => approve(m.applicationId)}
                disabled={m.status === "Aprobada"}
              >
                {m.status === "Aprobada" ? "Aprobada" : "Aprobar"}
              </button>
            )}
          </div>
        ))}

        {!items.length && (
          <div style={{ color: "#666" }}>No hay datos.</div>
        )}
      </div>
    </div>
  );
}
