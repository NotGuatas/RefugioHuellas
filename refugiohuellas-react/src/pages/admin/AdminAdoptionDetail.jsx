import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import { adminApi } from "../../api";

export default function AdminAdoptionDetail() {
  const { token } = useAuth();
  const { id } = useParams();

  const [data, setData] = useState(null);
  const [err, setErr] = useState("");
  const [ok, setOk] = useState("");

  useEffect(() => {
    async function load() {
      try {
        const d = await adminApi.adoptionDetail(token, id);
        setData(d);
      } catch (e) {
        setErr(e.message);
      }
    }
    load();
  }, [token, id]);

  async function approve() {
    setErr("");
    setOk("");
    try {
      const res = await adminApi.adoptionApprove(token, id);
      setOk(res.message);
      const fresh = await adminApi.adoptionDetail(token, id);
      setData(fresh);
    } catch (e) {
      setErr(e.message);
    }
  }

  return (
    <div style={{ display: "grid", gap: 12 }}>
      <Link to="/admin/adoptions">← Volver</Link>
      <h2 style={{ margin: 0 }}>Detalle solicitud #{id}</h2>

      {err && <div style={{ color: "crimson" }}>{err}</div>}
      {ok && <div style={{ color: "green" }}>{ok}</div>}

      {!data ? (
        <div>Cargando...</div>
      ) : (
        <>
          <div style={{ display: "grid", gap: 6 }}>
            <div><b>Usuario:</b> {data.userEmail || data.userId}</div>
            <div><b>Perro:</b> {data.dogName}</div>
            <div><b>Teléfono:</b> {data.phone}</div>
            <div><b>Compatibilidad:</b> {data.compatibilityScore}%</div>
            <div><b>Estado:</b> {data.status}</div>
            <div><b>Fecha:</b> {new Date(data.createdAt).toLocaleString()}</div>
          </div>

          <button onClick={approve} disabled={data.status === "Aprobada"}>
            {data.status === "Aprobada" ? "Ya aprobada" : "Aprobar"}
          </button>

          <h3 style={{ marginBottom: 0 }}>Respuestas</h3>
          <ul>
            {data.answers?.map((x, idx) => (
              <li key={idx}>
                {x.trait}: <b>{x.value}</b>
              </li>
            ))}
            {!data.answers?.length && <li>(Sin respuestas registradas)</li>}
          </ul>
        </>
      )}
    </div>
  );
}
