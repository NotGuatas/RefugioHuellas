import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { getDog } from "../api/api";

export default function DogDetail({ token }) {
  const { id } = useParams();
  const [dog, setDog] = useState(null);
  const [err, setErr] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let alive = true;

    (async () => {
      try {
        setErr("");
        setLoading(true);
        const data = await getDog(id, token);
        if (!alive) return;
        setDog(data);
      } catch (e) {
        if (!alive) return;
        setErr(e?.message || "Error");
      } finally {
        if (alive) setLoading(false);
      }
    })();

    return () => {
      alive = false;
    };
  }, [id, token]);

  if (err) return <div className="alert alert-danger">{err}</div>;
  if (loading || !dog) return <div className="text-muted">Cargando...</div>;

  const photo = dog.photoUrl || "https://via.placeholder.com/900x450";
  const desc = dog.description || dog.details || dog.summary || "";
  const health = dog.healthStatus || dog.healthState || dog.health || "";
  const sterilized = dog.isSterilized ?? dog.sterilized ?? dog.isNeutered;
  const intake = dog.intakeDate || dog.dateOfEntry || dog.createdAt;

  const yesNo = (v) => (v === true ? "Sí" : v === false ? "No" : "-");
  const formatDate = (value) => {
    if (!value) return "-";
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return String(value);
    return d.toLocaleDateString();
  };

  return (
    <div className="container py-3" style={{ maxWidth: 980 }}>
      <div className="mb-3">
        <Link to="/dogs" className="btn btn-outline-secondary btn-sm">
          ← Volver
        </Link>
      </div>

      <div className="card shadow-sm border-0">
        <div className="row g-0">
          <div className="col-md-6">
            <img
              src={photo}
              alt={dog.name}
              style={{
                width: "100%",
                height: "100%",
                minHeight: 320,
                objectFit: "cover",
                borderTopLeftRadius: 12,
                borderBottomLeftRadius: 12,
              }}
            />
          </div>

          <div className="col-md-6">
            <div className="card-body p-4">
              <div className="d-flex align-items-start justify-content-between gap-2">
                <div>
                  <h2 className="mb-1">{dog.name}</h2>
                  <div className="text-muted">
                    {dog.breed ? dog.breed : "Perro disponible para adopción"}
                  </div>
                </div>

                <span className="badge text-bg-success align-self-start">
                  Disponible
                </span>
              </div>

              {(desc || health) && <hr className="my-3" />}

              {desc && (
                <div className="mb-3">
                  <div className="fw-semibold mb-1">Descripción</div>
                  <div className="text-muted">{desc}</div>
                </div>
              )}

              <div className="row g-2 mb-3">
                <div className="col-6">
                  <div className="border rounded p-2">
                    <div className="text-muted small">Tamaño</div>
                    <div className="fw-semibold">{dog.size ?? "-"}</div>
                  </div>
                </div>
              
                <div className="col-12">
                  <div className="border rounded p-2">
                    <div className="text-muted small">Entorno ideal</div>
                    <div className="fw-semibold">{dog.idealEnvironment ?? "-"}</div>
                  </div>
                </div>
              </div>

              {(health || intake || sterilized !== undefined) && (
                <div className="mb-3">
                  <div className="fw-semibold mb-2">Información adicional</div>
                  <div className="d-grid gap-1 text-muted">
                    {health && (
                      <div>
                        <span className="fw-semibold text-body">Salud:</span>{" "}
                        {health}
                      </div>
                    )}
                    <div>
                      <span className="fw-semibold text-body">Esterilizado:</span>{" "}
                      {yesNo(sterilized)}
                    </div>
                    <div>
                      <span className="fw-semibold text-body">Ingreso:</span>{" "}
                      {formatDate(intake)}
                    </div>
                  </div>
                </div>
              )}

              <div className="d-flex flex-wrap gap-2">
                <Link to={`/adopt/${dog.id}`} className="btn btn-success">
                  Adoptar
                </Link>
                <Link to="/dogs" className="btn btn-outline-secondary">
                  Ver más perros
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="text-muted small mt-3">
        Si deseas adoptar, completa la solicitud y el refugio revisará tu compatibilidad.
      </div>
    </div>
  );
}
