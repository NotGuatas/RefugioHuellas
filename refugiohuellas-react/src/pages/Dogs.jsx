import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { getDogs, getDog } from "../api/api";

export default function Dogs({ token }) {
  const [dogs, setDogs] = useState([]);
  const [err, setErr] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let alive = true;

    (async () => {
      try {
        setErr("");
        setLoading(true);

        // 1) Lista básica
        const list = await getDogs();
        const base = Array.isArray(list) ? list : [];

        // 2) Enriquecer con detalle (para traer descripción/salud/esterilizado/fecha)
        //    Si no hay token, dejamos solo la lista.
        let full = base;

        if (token && base.length) {
          const details = await Promise.all(
            base.map(async (d) => {
              try {
                const det = await getDog(d.id, token);
                // Combina: lo que venga en detalle pisa lo de lista si existe
                return { ...d, ...det };
              } catch {
                return d; // si falla uno, no rompas todo
              }
            })
          );
          full = details;
        }

        if (!alive) return;
        setDogs(full);
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
  }, [token]);

  const yesNo = (v) => {
    if (v === true) return "Sí";
    if (v === false) return "No";
    // por si viene como string desde backend
    if (typeof v === "string") {
      const s = v.trim().toLowerCase();
      if (s === "si" || s === "sí" || s === "true") return "Sí";
      if (s === "no" || s === "false") return "No";
    }
    return "—";
  };

  const formatDate = (value) => {
    if (!value) return "—";
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return String(value);
    return d.toLocaleDateString();
  };

  const rows = useMemo(() => {
    return dogs.map((d) => {
      // intenta varias llaves comunes
      const description =
        d.description ?? d.descripcion ?? d.details ?? d.summary ?? "—";

      const health =
        d.healthStatus ??
        d.healthState ??
        d.health ??
        d.estadoSalud ??
        d.healthDescription ??
        "—";

      const sterilized =
        d.isSterilized ?? d.sterilized ?? d.isNeutered ?? d.esterilizado;

      const intake =
        d.intakeDate ?? d.dateOfEntry ?? d.fechaIngreso ?? d.createdAt;

      return {
        id: d.id,
        name: d.name,
        photoUrl: d.photoUrl,
        description,
        health,
        sterilized,
        intake,
      };
    });
  }, [dogs]);

  return (
    <div className="py-3">
      <div className="mb-3">
        <h2 className="mb-1">Perros en el refugio</h2>
        <div className="text-muted">
          Explora los peluditos que están esperando un hogar.
        </div>
      </div>

      {err && <div className="alert alert-danger">{err}</div>}

      <div className="card shadow-sm border-0">
        <div className="table-responsive">
          <table className="table align-middle mb-0">
            <thead className="table-light">
              <tr>
                <th style={{ width: 110 }}>Foto</th>
                <th style={{ width: 140 }}>Nombre</th>
                <th>Descripción</th>
                <th style={{ width: 260 }}>Estado de salud</th>
                <th style={{ width: 120 }}>Esterilizado</th>
                <th style={{ width: 140 }}>Fecha ingreso</th>
                <th style={{ width: 190, textAlign: "right" }}>Acciones</th>
              </tr>
            </thead>

            <tbody>
              {loading && (
                <tr>
                  <td colSpan={7} className="p-4 text-muted">
                    Cargando...
                  </td>
                </tr>
              )}

              {!loading && rows.length === 0 && (
                <tr>
                  <td colSpan={7} className="p-4 text-muted">
                    No hay perros disponibles.
                  </td>
                </tr>
              )}

              {!loading &&
                rows.map((d) => (
                  <tr key={d.id}>
                    <td>
                      <img
                        src={d.photoUrl || "https://via.placeholder.com/72"}
                        alt={d.name}
                        width={72}
                        height={72}
                        style={{ objectFit: "cover", borderRadius: 12 }}
                      />
                    </td>

                    <td className="fw-semibold">{d.name}</td>

                    <td className="text-muted" style={{ maxWidth: 520 }}>
                      {d.description}
                    </td>

                    <td className="text-muted" style={{ maxWidth: 320 }}>
                      {d.health}
                    </td>

                    <td className="fw-semibold">{yesNo(d.sterilized)}</td>

                    <td>{formatDate(d.intake)}</td>

                    <td style={{ textAlign: "right" }}>
                      <div className="d-inline-flex gap-2">
                        <Link
                          to={`/dogs/${d.id}`}
                          className="btn btn-outline-secondary btn-sm"
                        >
                          Ver
                        </Link>
                        <Link
                          to={`/adopt/${d.id}`}
                          className="btn btn-success btn-sm"
                        >
                          Adoptar
                        </Link>
                      </div>
                    </td>
                  </tr>
                ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
