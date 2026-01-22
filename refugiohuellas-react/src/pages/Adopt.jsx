import { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { adoptionsApi, dogsApi } from "../api";

export default function Adopt({ token }) {
  const { dogId } = useParams();
  const nav = useNavigate();

  const [dog, setDog] = useState(null);

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [phone, setPhone] = useState("");

  const [existing, setExisting] = useState(null); // <- solicitud ya enviada para este perro
  const [err, setErr] = useState("");
  const [ok, setOk] = useState("");
  const [loading, setLoading] = useState(false);

  // Si no hay token, manda a login (manteniendo retorno)
  useEffect(() => {
    if (!token) nav(`/login?return=/adopt/${dogId}`);
  }, [token, nav, dogId]);

  // Cargar perro + verificar si ya existe solicitud del usuario para ese perro
  useEffect(() => {
    if (!token) return;

    let alive = true;
    setErr("");
    setOk("");

    async function load() {
      try {
        const d = await dogsApi.get(dogId, token);
        if (!alive) return;
        setDog(d);

        const mine = await adoptionsApi.my(token);
        if (!alive) return;

        const already = (mine || []).find(
          (a) => String(a.dogId) === String(dogId)
        );

        setExisting(already || null);

        // (Opcional) si tu API devuelve nombres ya guardados, podrías precargar:
        // if (already?.firstName) setFirstName(already.firstName);
      } catch (e) {
        if (!alive) return;
        setErr(e.message);
      }
    }

    load();
    return () => {
      alive = false;
    };
  }, [dogId, token]);

  async function submit(e) {
    e.preventDefault();
    setErr("");
    setOk("");
    setLoading(true);

    try {
      const res = await adoptionsApi.create(token, {
        dogId: Number(dogId),
        firstName,
        lastName,
        phone,
      });

      // Mensaje tipo MVC: "Solicitud enviada..." + compatibilidad + estado
      const compat = res?.compatibilityScore ?? res?.compatibility ?? "?";
      const status = res?.status ?? "Pendiente";
      const dogName = dog?.name ?? "";

      setOk(
        `Solicitud enviada para ${dogName}. Compatibilidad: ${compat}%. Estado: ${status}.`
      );

      // Refrescar para que aparezca el banner "Ya enviaste..." y se bloquee el form
      const mine = await adoptionsApi.my(token);
      const already = (mine || []).find(
        (a) => String(a.dogId) === String(dogId)
      );
      setExisting(already || existing);

      // Igual que tu lógica: ir a "mis solicitudes"
      setTimeout(() => nav("/my-applications"), 600);
    } catch (e) {
      const msg = e.message || "Error";
      setErr(msg);

      // Si backend dice "perfil requerido", manda a profile
      if (msg.toLowerCase().includes("perfil")) {
        nav("/profile");
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <div style={{ display: "grid", gap: 12, maxWidth: 520 }}>
      <Link to={`/dogs/${dogId}`}>← Volver</Link>

      <h2 style={{ margin: 0 }}>
        Solicitud de adopción {dog ? `– ${dog.name}` : ""}
      </h2>

      {/* Banner tipo MVC si ya existe */}
      {existing && (
        <div style={{ background: "#e8f6ff", padding: 12, borderRadius: 10 }}>
          Ya enviaste una solicitud para{" "}
          <b>{existing.dogName || dog?.name}</b>. Compatibilidad:{" "}
          <b>{existing.compatibilityScore}%</b>. Estado:{" "}
          <b>{existing.status}</b>.
        </div>
      )}

      {err && <div style={{ color: "crimson" }}>{err}</div>}
      {ok && <div style={{ color: "green" }}>{ok}</div>}

      <form onSubmit={submit} style={{ display: "grid", gap: 10 }}>
        <label>
          Nombre
          <input
            value={firstName}
            onChange={(e) => setFirstName(e.target.value)}
            className="form-control"
            placeholder="Ej: Joseph"
            disabled={!!existing}
          />
        </label>

        <label>
          Apellido
          <input
            value={lastName}
            onChange={(e) => setLastName(e.target.value)}
            className="form-control"
            placeholder="Ej: Flores"
            disabled={!!existing}
          />
        </label>

        <label>
          Teléfono (09xxxxxxxx)
          <input
            value={phone}
            onChange={(e) => setPhone(e.target.value)}
            className="form-control"
            placeholder="0991234567"
            disabled={!!existing}
          />
        </label>

        <button className="btn btn-success" disabled={loading || !!existing}>
          {existing ? "Solicitud ya enviada" : loading ? "Enviando..." : "Enviar solicitud"}
        </button>
      </form>
    </div>
  );
}
