import { useCallback, useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import { adminApi } from "../../api";
import { motion } from "framer-motion";

function SkeletonRow() {
  return (
    <tr>
      <td colSpan={8} style={{ padding: 16 }}>
        <div
          style={{
            height: 18,
            width: "100%",
            background: "#eee",
            borderRadius: 10,
          }}
        />
      </td>
    </tr>
  );
}

function EnergyBadge({ value }) {
  const n = Number(value ?? 0);
  const cls = n >= 4 ? "bg-success" : n === 3 ? "bg-warning text-dark" : "bg-secondary";
  return (
    <span className={`badge ${cls}`} style={{ borderRadius: 999 }}>
      {Number.isFinite(n) ? n : "-"}
    </span>
  );
}

function SizeBadge({ value }) {
  const s = (value || "").toLowerCase();
  const cls = s.includes("peque")
    ? "bg-info text-dark"
    : s.includes("median")
    ? "bg-primary"
    : s.includes("grand")
    ? "bg-dark"
    : "bg-secondary";
  return (
    <span className={`badge ${cls}`} style={{ borderRadius: 999 }}>
      {value || "-"}
    </span>
  );
}

export default function AdminDogs() {
  const { token } = useAuth();

  const [dogs, setDogs] = useState([]);
  const [err, setErr] = useState("");
  const [loading, setLoading] = useState(false);

  const MotionDiv = motion.div;

  const load = useCallback(async () => {
    setErr("");
    setLoading(true);
    try {
      const data = await adminApi.dogsList(token);
      setDogs(Array.isArray(data) ? data : []);
    } catch (e) {
      setErr(e?.message || "Error al cargar perros");
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => {
    load();
  }, [load]);

  const total = useMemo(() => dogs.length, [dogs]);

  return (
    <div className="container" style={{ maxWidth: 1100 }}>
      {/* Header */}
      <MotionDiv
        initial={{ opacity: 0, y: 6 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.18 }}
        className="d-flex align-items-start justify-content-between flex-wrap gap-2"
        style={{ marginTop: 10 }}
      >
        <div>
          <h2 className="mb-1">Admin: Perros</h2>
          <div className="text-muted">{loading ? "Cargando..." : `${total} registros`}</div>
        </div>

        <MotionDiv whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
          <Link
            to="/admin/dogs/new"
            className="btn btn-success btn-sm"
            style={{ borderRadius: 999, paddingInline: 14, fontWeight: 600 }}
          >
            + Nuevo perro
          </Link>
        </MotionDiv>
      </MotionDiv>

      {/* Error */}
      {err && (
        <div className="alert alert-danger mt-3 mb-0" role="alert">
          {err}
        </div>
      )}

      {/* Table */}
      <div className="card border-0 shadow-sm mt-3" style={{ borderRadius: 16 }}>
        <div className="table-responsive">
          <table className="table table-hover align-middle mb-0">
            <thead className="table-light">
              <tr>
                <th style={{ width: 70 }}>ID</th>
                <th style={{ width: 110 }}>Foto</th>
                <th>Nombre</th>
                <th>Raza</th>
                <th style={{ width: 140 }}>Tamaño</th>
                <th style={{ width: 110 }}>Energía</th>
                <th>Origen</th>
                <th style={{ width: 190 }}>Acciones</th>
              </tr>
            </thead>

            <tbody>
              {loading && (
                <>
                  <SkeletonRow />
                  <SkeletonRow />
                  <SkeletonRow />
                </>
              )}

              {!loading && dogs.length === 0 && (
                <tr>
                  <td colSpan="8" className="text-muted" style={{ padding: 16 }}>
                    No hay perros registrados.
                  </td>
                </tr>
              )}

              {!loading &&
                dogs.map((d) => (
                  <tr key={d.id} style={{ verticalAlign: "middle" }}>
                    <td className="text-muted">{d.id}</td>

                    <td>
                      <img
                        src={d.photoUrl || "https://via.placeholder.com/80x50"}
                        alt={d.name}
                        style={{
                          width: 84,
                          height: 54,
                          objectFit: "cover",
                          borderRadius: 12,
                          background: "#eee",
                          border: "1px solid rgba(0,0,0,.06)",
                        }}
                      />
                    </td>

                    <td className="fw-semibold">{d.name}</td>
                    <td className="text-muted">{d.breed || "-"}</td>

                    <td>
                      <SizeBadge value={d.size} />
                    </td>

                    <td>
                      <EnergyBadge value={d.energyLevel} />
                    </td>

                    <td className="text-muted">{d.originTypeName || "-"}</td>

                    <td>
                      <div className="d-flex gap-2 flex-wrap">
                        <MotionDiv whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                          <Link
                            to={`/admin/dogs/${d.id}/edit`}
                            className="btn btn-outline-primary btn-sm"
                            style={{ borderRadius: 999, paddingInline: 12, fontWeight: 600 }}
                          >
                            Editar
                          </Link>
                        </MotionDiv>

                        <MotionDiv whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                          <Link
                            to={`/admin/dogs/${d.id}/delete`}
                            className="btn btn-outline-danger btn-sm"
                            style={{ borderRadius: 999, paddingInline: 12, fontWeight: 600 }}
                          >
                            Eliminar
                          </Link>
                        </MotionDiv>
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
