import { useCallback, useEffect, useMemo, useState } from "react";
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

export default function AdminBestMatches() {
  const { token } = useAuth();

  const [items, setItems] = useState([]);
  const [err, setErr] = useState("");
  const [ok, setOk] = useState("");
  const [loading, setLoading] = useState(false);
  const [busyId, setBusyId] = useState(null);

  const MotionDiv = motion.div;

  const load = useCallback(async () => {
    setErr("");
    setOk("");
    setLoading(true);
    try {
      const data = await adminApi.matchesBest(token);
      setItems(Array.isArray(data) ? data : []);
    } catch (e) {
      setErr(e?.message || "Error al cargar coincidencias");
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => {
    if (!token) return;
    load();
  }, [load, token]);

  async function approve(applicationId) {
    setErr("");
    setOk("");
    setBusyId(applicationId);
    try {
      const res = await adminApi.matchApprove(token, applicationId);
      setOk(res?.message || "Solicitud aprobada");
      await load();
    } catch (e) {
      setErr(e?.message || "Error al aprobar");
    } finally {
      setBusyId(null);
    }
  }

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
          <h2 className="mb-1">Admin: Mejores coincidencias</h2>
          <div className="text-muted">{loading ? "Cargando..." : `${total} registros`}</div>
        </div>
      </MotionDiv>

      {/* Alerts */}
      {err && (
        <div className="alert alert-danger mt-3 mb-0" role="alert">
          {err}
        </div>
      )}
      {ok && (
        <div className="alert alert-success mt-3 mb-0" role="alert">
          {ok}
        </div>
      )}

      {/* List */}
      <div className="mt-3" style={{ display: "grid", gap: 10 }}>
        {loading && (
          <>
            <div className="card border-0 shadow-sm" style={{ borderRadius: 16 }}>
              <div className="card-body" style={{ height: 78, background: "#eee", borderRadius: 16 }} />
            </div>
            <div className="card border-0 shadow-sm" style={{ borderRadius: 16 }}>
              <div className="card-body" style={{ height: 78, background: "#eee", borderRadius: 16 }} />
            </div>
            <div className="card border-0 shadow-sm" style={{ borderRadius: 16 }}>
              <div className="card-body" style={{ height: 78, background: "#eee", borderRadius: 16 }} />
            </div>
          </>
        )}

        {!loading && items.length === 0 && (
          <div className="card border-0 shadow-sm" style={{ borderRadius: 16 }}>
            <div className="card-body text-muted">No hay datos.</div>
          </div>
        )}

        {!loading &&
          items.map((m) => {
            const approved = (m.status || "").toLowerCase().includes("aprob");
            const hasApp = !!m.applicationId;

            return (
              <div key={m.dogId} className="card border-0 shadow-sm" style={{ borderRadius: 16 }}>
                <div className="card-body d-flex align-items-center justify-content-between flex-wrap gap-3">
                  {/* Left: image + dog info */}
                  <div className="d-flex align-items-center gap-3">
                    <img
                      src={m.dogPhotoUrl || "https://via.placeholder.com/100x70"}
                      alt={m.dogName}
                      style={{
                        width: 96,
                        height: 64,
                        objectFit: "cover",
                        borderRadius: 12,
                        background: "#eee",
                        border: "1px solid rgba(0,0,0,.06)",
                      }}
                    />

                    <div>
                      <div className="fw-bold">{m.dogName}</div>
                      <div className="text-muted small">
                        Ingreso:{" "}
                        {m.dogIntakeDate ? new Date(m.dogIntakeDate).toLocaleDateString() : "-"}
                      </div>

                      {hasApp ? (
                        <div className="text-muted small mt-2" style={{ lineHeight: 1.45 }}>
                          <div>
                            Ganador: <b className="text-dark">{m.userEmail}</b>
                          </div>
                          <div className="d-flex align-items-center gap-2 flex-wrap">
                            <span>Compat:</span> <ScoreBadge score={m.compatibilityScore} />
                            <span className="ms-1">Estado:</span> <StatusBadge status={m.status} />
                          </div>
                        </div>
                      ) : (
                        <div className="text-muted small mt-2">Sin solicitudes</div>
                      )}
                    </div>
                  </div>

                  {/* Right: action */}
                  {hasApp && (
                    <MotionDiv whileHover={{ scale: approved ? 1 : 1.02 }} whileTap={{ scale: approved ? 1 : 0.98 }}>
                      <button
                        className={approved ? "btn btn-outline-secondary btn-sm" : "btn btn-success btn-sm"}
                        style={{ borderRadius: 999, paddingInline: 14, fontWeight: 700 }}
                        onClick={() => approve(m.applicationId)}
                        disabled={approved || busyId === m.applicationId}
                      >
                        {approved
                          ? "Aprobada"
                          : busyId === m.applicationId
                          ? "Aprobando..."
                          : "Aprobar"}
                      </button>
                    </MotionDiv>
                  )}
                </div>
              </div>
            );
          })}
      </div>
    </div>
  );
}
