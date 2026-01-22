import { useEffect, useMemo, useState } from "react";
import { getTraits, getMyTraits, saveMyTraits } from "../api/api";
import { useAuth } from "../auth/AuthContext";

export default function Profile({ token }) {
  const { user } = useAuth();

  const email =
    user?.email || user?.userName || user?.username || user?.name || "Usuario";

  const [showForm, setShowForm] = useState(false);

  const [traits, setTraits] = useState([]);
  const [values, setValues] = useState({}); // { traitId: value }
  const [msg, setMsg] = useState("");
  const [err, setErr] = useState("");

  // Carga de traits + mis respuestas SOLO cuando se abre la encuesta
  useEffect(() => {
    if (!showForm) return;

    setMsg("");
    setErr("");

    Promise.all([getTraits(), getMyTraits(token)])
      .then(([t, mine]) => {
        setTraits(t);
        const map = {};
        for (const r of mine) map[r.traitId] = r.value;
        setValues(map);
      })
      .catch((e) => setErr(e.message));
  }, [token, showForm]);

  const allAnswered = useMemo(() => {
    if (!traits.length) return false;
    return traits.every((t) => values[t.id] >= 1 && values[t.id] <= 5);
  }, [traits, values]);

  const setVal = (traitId, v) => {
    setValues((prev) => ({ ...prev, [traitId]: v }));
  };

  const onSave = async () => {
    setMsg("");
    setErr("");
    try {
      const items = traits.map((t) => ({
        traitId: t.id,
        value: values[t.id] ?? 3,
      }));
      await saveMyTraits(token, items);
      setMsg("Perfil guardado ✅ Ya puedes ver compatibilidad.");
    } catch (e) {
      setErr(e.message);
    }
  };

  // Pantalla inicial (sin encuesta)
  if (!showForm) {
    return (
      <div style={{ maxWidth: 900 }}>
        <h2>Mi perfil</h2>
        <p style={{ color: "#666" }}>
          Completa la encuesta para calcular compatibilidad.
        </p>

        {err && (
          <div style={{ background: "#ffd9d9", padding: 10, borderRadius: 10 }}>
            {err}
          </div>
        )}
        {msg && (
          <div style={{ background: "#d9ffdf", padding: 10, borderRadius: 10 }}>
            {msg}
          </div>
        )}

        <div
          style={{
            border: "1px solid #ddd",
            borderRadius: 14,
            padding: 14,
            marginTop: 12,
            display: "grid",
            gap: 10,
          }}
        >
          <div>
            <div style={{ fontWeight: 700 }}>Correo</div>
            <div style={{ color: "#333" }}>{email}</div>
          </div>

          <button onClick={() => setShowForm(true)} style={{ width: 200 }}>
            Llenar encuesta
          </button>

          <span style={{ color: "#888", fontSize: 13 }}>
          </span>
        </div>
      </div>
    );
  }

  // Pantalla con encuesta ( UI original de .net )
  return (
    <div style={{ maxWidth: 900 }}>
      <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
        <h2 style={{ margin: 0 }}>Mi perfil</h2>
        <button onClick={() => setShowForm(false)} style={{ marginLeft: "auto" }}>
          Volver
        </button>
      </div>

      <p style={{ color: "#666" }}>
        Responde estas preguntas (1 = muy bajo / 5 = muy alto). Esto se usa para
        calcular compatibilidad.
      </p>

      {err && (
        <div style={{ background: "#ffd9d9", padding: 10, borderRadius: 10 }}>
          {err}
        </div>
      )}
      {msg && (
        <div style={{ background: "#d9ffdf", padding: 10, borderRadius: 10 }}>
          {msg}
        </div>
      )}

      <div style={{ display: "grid", gap: 12, marginTop: 12 }}>
        {traits.map((t) => (
          <div
            key={t.id}
            style={{ border: "1px solid #333", borderRadius: 14, padding: 12 }}
          >
            <div style={{ fontWeight: 700 }}>{t.name}</div>
            <div style={{ color: "#aaa", fontSize: 14, marginTop: 4 }}>
              {t.prompt || "Selecciona tu nivel"}
            </div>

            <div
              style={{
                display: "flex",
                alignItems: "center",
                gap: 10,
                marginTop: 10,
              }}
            >
              <input
                type="range"
                min="1"
                max="5"
                value={values[t.id] ?? 3}
                onChange={(e) => setVal(t.id, Number(e.target.value))}
                style={{ width: 260 }}
              />
              <span style={{ fontWeight: 700 }}>{values[t.id] ?? 3}</span>
            </div>
          </div>
        ))}
      </div>

      <div style={{ marginTop: 16, display: "flex", gap: 12, alignItems: "center" }}>
        <button onClick={onSave} disabled={!traits.length}>
          Guardar perfil
        </button>

        {!allAnswered && traits.length > 0 && (
          <span style={{ color: "#aaa" }}>
            Tip: aunque guardes, intenta responder todo para mejores resultados.
          </span>
        )}
      </div>
    </div>
  );
}
