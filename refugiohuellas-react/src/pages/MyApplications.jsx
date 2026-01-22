import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { adoptionsApi } from "../api";

export default function MyApplications({ token }) {
  const [items, setItems] = useState([]);
  const [err, setErr] = useState("");

  useEffect(() => {
    if (!token) return;

    let alive = true;

    (async () => {
      try {
        const data = await adoptionsApi.my(token);
        if (!alive) return;
        setItems(data);
        setErr("");
      } catch (e) {
        if (!alive) return;
        setErr(e.message || "Error");
      }
    })();

    return () => {
      alive = false;
    };
  }, [token]);

  return (
    <div style={{ display: "grid", gap: 12 }}>
      <h2 style={{ margin: 0 }}>Mis solicitudes</h2>

      {err && <div style={{ color: "crimson" }}>{err}</div>}

      {!items.length ? (
        <div>No tienes solicitudes aún.</div>
      ) : (
        <div style={{ display: "grid", gap: 10 }}>
          {items.map((a) => (
            <div
              key={a.id}
              style={{
                border: "1px solid #ddd",
                borderRadius: 12,
                padding: 12,
                display: "grid",
                gap: 6,
              }}
            >
              <div style={{ fontWeight: 700 }}>
                {a.dogName} — {a.status}
              </div>
              <div>Compatibilidad: {a.compatibilityScore}%</div>
              <div>Teléfono: {a.phone}</div>
              <div>
                <Link to={`/dogs/${a.dogId}`}>Ver perro</Link>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
