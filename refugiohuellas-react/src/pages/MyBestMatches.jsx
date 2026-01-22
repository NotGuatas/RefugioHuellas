import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getMyBestMatches } from "../api/api";

export default function MyBestMatches({ token }) {
  const [items, setItems] = useState([]);
  const [err, setErr] = useState("");
  const [loading, setLoading] = useState(true); // ✅ nuevo

  useEffect(() => {
    let alive = true;

    setErr("");
    setLoading(true);

    getMyBestMatches(token)
      .then((data) => {
        if (!alive) return;
        setItems(Array.isArray(data) ? data : []);
      })
      .catch((e) => {
        if (!alive) return;
        setErr(e.message);
      })
      .finally(() => {
        if (!alive) return;
        setLoading(false);
      });

    return () => {
      alive = false;
    };
  }, [token]);

  if (loading) return <div className="container py-4">Cargando coincidencias...</div>;
  if (err) return <div className="alert alert-danger">{err}</div>;

  return (
    <div style={{ maxWidth: 1100 }}>
      <h2>Mis mejores coincidencias</h2>
      <p style={{ color: "#666" }}>
        Basado en tu formulario de compatibilidad, estos son los perritos que mejor encajan contigo.
      </p>

      <div className="row g-3">
        {items.map((x) => (
          <div className="col-md-4" key={x.dogId}>
            <div className="card h-100">
              <img
                src={x.photoUrl || "https://via.placeholder.com/600x300"}
                className="card-img-top"
                alt={x.dogName}
                style={{ height: 220, objectFit: "cover" }}
              />
              <div className="card-body">
                <h5 className="card-title">{x.dogName}</h5>
                <div><b>Compatibilidad:</b> {x.score}%</div>
                <div><b>Tamaño:</b> {x.sizeLabel}</div>
                <div><b>Nivel de energía:</b> {x.energy}/5</div>
              </div>
              <div className="card-footer d-flex gap-2">
                <Link className="btn btn-outline-primary btn-sm" to={`/dogs/${x.dogId}`}>
                  Ver detalle
                </Link>
                <Link className="btn btn-success btn-sm" to={`/adopt/${x.dogId}`}>
                  Adoptar
                </Link>
              </div>
            </div>
          </div>
        ))}
      </div>

      {items.length === 0 && (
        <div className="alert alert-warning mt-3">
          Aún no hay coincidencias. Completa tu perfil para generar compatibilidad.
          <div className="mt-2">
            <Link to="/profile" className="btn btn-primary btn-sm">
              Ir a Mi perfil
            </Link>
          </div>
        </div>
      )}
    </div>
  );
}
