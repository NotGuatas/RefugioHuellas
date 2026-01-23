import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import { adminApi } from "../../api";
import { motion } from "framer-motion";

function StatusBadge({ status }) {
  const s = (status || "").toLowerCase();

  const isApproved = s.includes("aprob") || s.includes("approved");
  const isPending = s.includes("pend") || s.includes("pending");
  const isRejected =
    s.includes("rech") || s.includes("deneg") || s.includes("recha") || s.includes("rejected");

  const cls = isApproved
    ? "bg-success"
    : isPending
    ? "bg-warning text-dark"
    : isRejected
    ? "bg-danger"
    : "bg-secondary";

  return (
    <span className={`badge ${cls}`} style={{ borderRadius: 999 }}>
      {status || "-"}
    </span>
  );
}

function ScoreBadge({ score }) {
  const n = Number(score ?? 0);
  const cls = n >= 70 ? "bg-success" : n >= 50 ? "bg-warning text-dark" : "bg-danger";
  return (
    <span className={`badge ${cls}`} style={{ borderRadius: 999 }}>
      {Number.isFinite(n) ? `${n}%` : "-"}
    </span>
  );
}

function SkeletonRow() {
  return (
    <tr>
      <td colSpan={7} style={{ padding: 16 }}>
        <div style={{ height: 18, width: "100%", background: "#eee", borderRadius: 10 }} />
      </td>
    </tr>
  );
}

export default function AdminAdoptions() {
  const { token } = useAuth();
  const [items, setItems] = useState([]);
  const [err, setErr] = useState("");
  const [loading, setLoading] = useState(false);

  const MotionDiv = motion.div;

  useEffect(() => {
    if (!token) return;

    let alive = true;

    (async () => {
      try {
        setErr("");
        setLoading(true);
        const data = await adminApi.adoptionsList(token);
        if (!alive) return;
        setItems(Array.isArray(data) ? data : []);
      } catch (e) {
        if (!alive) return;
        setErr(e?.message || "Error al cargar solicitudes");
      } finally {
        if (alive) setLoading(false);
      }
    })();

    return () => {
      alive = false;
    };
  }, [token]);

  const total = useMemo(() => items.length, [items]);

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
          <h2 className="mb-1">Admin: Solicitudes</h2>
          <div className="text-muted">{loading ? "Cargando..." : `${total} registros`}</div>
        </div>
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
                <th>Usuario</th>
                <th>Perro</th>
                <th style={{ width: 110 }}>Compat</th>
                <th style={{ width: 140 }}>Estado</th>
                <th style={{ width: 190 }}>Fecha</th>
                <th style={{ width: 120 }}></th>
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

              {!loading && items.length === 0 && (
                <tr>
                  <td colSpan="7" className="text-muted" style={{ padding: 16 }}>
                    No hay solicitudes.
                  </td>
                </tr>
              )}

              {!loading &&
                items.map((a) => (
                  <tr key={a.id} style={{ verticalAlign: "middle" }}>
                    <td className="text-muted">{a.id}</td>
                    <td className="text-muted">{a.userEmail || a.userId || "-"}</td>
                    <td className="fw-semibold">{a.dogName || "-"}</td>
                    <td>
                      <ScoreBadge score={a.compatibilityScore} />
                    </td>
                    <td>
                      <StatusBadge status={a.status} />
                    </td>
                    <td className="text-muted">
                      {a.createdAt ? new Date(a.createdAt).toLocaleString() : "-"}
                    </td>
                    <td className="text-end">
                      <MotionDiv whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                        <Link
                          to={`/admin/adoptions/${a.id}`}
                          className="btn btn-outline-primary btn-sm"
                          style={{ borderRadius: 999, paddingInline: 12, fontWeight: 600 }}
                        >
                          Ver
                        </Link>
                      </MotionDiv>
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
