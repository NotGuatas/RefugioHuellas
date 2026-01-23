import { useState } from "react";
import { Link } from "react-router-dom";
import { getMyBestMatches } from "../api/api";

export default function MyBestMatches({ token }) {
  const [items, setItems] = useState([]);
  const [err, setErr] = useState("");
  const [loading, setLoading] = useState(false);
  const [revealed, setRevealed] = useState(false);

  async function reveal() {
    try {
      setErr("");
      setLoading(true);
      const data = await getMyBestMatches(token);
      setItems(data);
      setRevealed(true);
    } catch (e) {
      setErr(e.message);
    } finally {
      setLoading(false);
    }
  }

  if (err) return <div className="alert alert-danger">{err}</div>;

  return (
    <div style={{ maxWidth: 1100 }}>
      <h2>Mis mejores coincidencias</h2>
      <p style={{ color: "#666" }}>
        Presiona el botón para revelar tus 3 mejores matches.
      </p>

      {!revealed && (
        <div className="card p-4 rh-card">
          <div className="d-flex align-items-center justify-content-between flex-wrap gap-2">
            <div>
              <div className="fw-bold">¿Listo para ver tus coincidencias?</div>
              <div style={{ color: "#666" }}>
                Calcularemos tu compatibilidad y te mostraremos los mejores.
              </div>
            </div>

            <button className="btn btn-success" onClick={reveal} disabled={loading}>
              {loading ? "Calculando..." : "Revelar coincidencias"}
            </button>
          </div>
        </div>
      )}

      {revealed && (
        <>
          <div className="row g-3 mt-1">
            {items.map((x) => (
              <div className="col-md-4" key={x.dogId}>
                <div className="card h-100 rh-card">
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

          <div className="mt-3">
            <button className="btn btn-outline-secondary btn-sm" onClick={() => setRevealed(false)}>
              Ocultar coincidencias
            </button>
          </div>
        </>
      )}
    </div>
  );
}
