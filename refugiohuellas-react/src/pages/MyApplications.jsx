import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { adoptionsApi } from "../api";
import { motion, AnimatePresence } from "framer-motion";

function StatusBadge({ status }) {
  const s = (status || "").toLowerCase();


  const isApproved = s.includes("aprob");
  const isPending = s.includes("pend");
  const isRejected = s.includes("rech") || s.includes("deneg") || s.includes("recha");

  const cls = isApproved
    ? "bg-success"
    : isPending
    ? "bg-warning text-dark"
    : isRejected
    ? "bg-danger"
    : "bg-secondary";

  return (
    <span className={`badge ${cls}`} style={{ borderRadius: 999 }}>
      {status}
    </span>
  );
}

function ScoreBadge({ score }) {
  const n = Number(score ?? 0);
  const cls = n >= 70 ? "bg-success" : n >= 50 ? "bg-warning text-dark" : "bg-danger";
  return (
    <span className={`badge ${cls}`} style={{ borderRadius: 999 }}>
      {n}%
    </span>
  );
}

function SkeletonRow() {
  return (
    <div className="card border-0 shadow-sm" style={{ borderRadius: 16 }}>
      <div className="card-body d-flex align-items-center justify-content-between gap-3">
        <div style={{ width: "60%" }}>
          <div style={{ height: 14, width: "45%", background: "#eee", borderRadius: 8 }} />
          <div style={{ height: 12, width: "60%", background: "#eee", borderRadius: 8, marginTop: 10 }} />
        </div>
        <div style={{ width: 120, height: 32, background: "#eee", borderRadius: 999 }} />
      </div>
    </div>
  );
}

const listVariants = {
  hidden: { opacity: 0 },
  show: { opacity: 1, transition: { staggerChildren: 0.06, delayChildren: 0.03 } },
};

const itemVariants = {
  hidden: { opacity: 0, y: 8 },
  show: { opacity: 1, y: 0, transition: { duration: 0.18 } },
};

export default function MyApplications({ token }) {
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
        const data = await adoptionsApi.my(token);
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
          <h2 className="mb-1">Mis solicitudes</h2>
          <div className="text-muted">{loading ? "Cargando..." : `${items.length} solicitudes`}</div>
        </div>
      </MotionDiv>

      {/* Error */}
      {err && (
        <div className="alert alert-danger mt-3 mb-0" role="alert">
          {err}
        </div>
      )}

      {/* Loading */}
      {loading && (
        <div className="mt-3" style={{ display: "grid", gap: 10 }}>
          <SkeletonRow />
          <SkeletonRow />
          <SkeletonRow />
          <SkeletonRow />
        </div>
      )}

      {/* Empty */}
      {!loading && !err && items.length === 0 && (
        <div className="card border-0 shadow-sm mt-3" style={{ borderRadius: 16 }}>
          <div className="card-body">
            <div className="fw-bold mb-1">No tienes solicitudes aún</div>
            <div className="text-muted">Explora los perros disponibles y envía tu primera solicitud.</div>
            <div className="mt-3">
              <Link className="btn btn-success btn-sm" to="/dogs">
                Ver perros
              </Link>
            </div>
          </div>
        </div>
      )}

      {/* List */}
      {!loading && !err && items.length > 0 && (
        <AnimatePresence mode="wait">
          <MotionDiv
            key="list"
            className="mt-3"
            style={{ display: "grid", gap: 10 }}
            variants={listVariants}
            initial="hidden"
            animate="show"
            exit={{ opacity: 0 }}
          >
            {items.map((a) => (
              <MotionDiv
                key={a.id}
                variants={itemVariants}
                className="card border-0 shadow-sm"
                style={{ borderRadius: 16 }}
              >
                <div className="card-body d-flex align-items-center justify-content-between flex-wrap gap-3">
                  <div style={{ minWidth: 260 }}>
                    <div className="d-flex align-items-center gap-2 flex-wrap">
                      <div className="fw-bold">{a.dogName}</div>
                      <StatusBadge status={a.status} />
                    </div>

                    <div className="text-muted small mt-2" style={{ lineHeight: 1.5 }}>
                      <div className="d-flex align-items-center gap-2 flex-wrap">
                        <span>Compatibilidad:</span>
                        <ScoreBadge score={a.compatibilityScore} />
                      </div>
                      <div>Teléfono: {a.phone}</div>
                    </div>
                  </div>

                  <MotionDiv whileHover={{ scale: 1.03 }} whileTap={{ scale: 0.97 }}>
                    <Link
                      to={`/dogs/${a.dogId}`}
                      className="btn btn-outline-primary btn-sm d-flex align-items-center gap-1"
                      style={{ borderRadius: 999, paddingInline: 14, fontWeight: 600 }}
                    >
                       Ver perro
                    </Link>
                  </MotionDiv>
                </div>
              </MotionDiv>
            ))}
          </MotionDiv>
        </AnimatePresence>
      )}
    </div>
  );
}
