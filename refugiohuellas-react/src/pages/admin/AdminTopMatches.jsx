import { useCallback, useEffect, useMemo, useState } from "react";
import { useAuth } from "../../auth/AuthContext";
import { adminApi } from "../../api";
import { motion } from "framer-motion";

function StatusBadge({ status }) {
  const s = (status || "").toLowerCase();

  const isApproved = s.includes("aprob");
  const isPending = s.includes("pend");
  const isRejected = s.includes("rech") || s.includes("deneg");

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

export default function AdminTopMatches() {
  const { token } = useAuth();

  const [data, setData] = useState(null);
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
      const res = await adminApi.matchesTop(token, 7);
      setData(res);
    } catch (e) {
      setErr(e?.message || "Error al cargar candidatos");
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
      setOk(res?.message || "Aprobado");
      await load();
    } catch (e) {
      setErr(e?.message || "Error al aprobar");
    } finally {
      setBusyId(null);
    }
  }

  const totalDogs = useMemo(() => (data?.dogs?.length ?? 0), [data]);

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
          <h2 className="mb-1">Admin: Mejores candidatos</h2>
          <div className="text-muted">
            {loading ? "Cargando..." : `${totalDogs} perros en ventana (7 días)`}
          </div>
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

      {/* Content */}
      <div className="mt-3" style={{ display: "grid", gap: 12 }}>
        {loading && (
          <>
            <div className="card border-0 shadow-sm" style={{ borderRadius: 16 }}>
              <div className="card-body" style={{ height: 120, background: "#eee", borderRadius: 16 }} />
            </div>
            <div className="card border-0 shadow-sm" style={{ borderRadius: 16 }}>
              <div className="card-body" style={{ height: 120, background: "#eee", borderRadius: 16 }} />
            </div>
          </>
        )}

        {!loading && !data && (
          <div className="card border-0 shadow-sm" style={{ borderRadius: 16 }}>
            <div className="card-body text-muted">Cargando...</div>
          </div>
        )}

        {!loading && data && data.dogs?.length === 0 && (
          <div className="card border-0 shadow-sm" style={{ borderRadius: 16 }}>
            <div className="card-body text-muted">No hay perros en la ventana.</div>
          </div>
        )}

        {!loading &&
          data?.dogs?.map((d) => (
            <div key={d.dogId} className="card border-0 shadow-sm" style={{ borderRadius: 16 }}>
              <div className="card-body">
                {/* Dog header */}
                <div className="d-flex align-items-center justify-content-between flex-wrap gap-3">
                  <div className="d-flex align-items-center gap-3">
                    <img
                      src={d.dogPhotoUrl || "https://via.placeholder.com/100x70"}
                      alt={d.dogName}
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
                      <div className="fw-bold">{d.dogName}</div>
                      <div className="text-muted small">
                        Ingreso: {d.dogIntakeDate ? new Date(d.dogIntakeDate).toLocaleString() : "-"}
                      </div>
                    </div>
                  </div>

                  <div className="d-flex align-items-center gap-2 flex-wrap">
                    <span className="badge bg-light text-dark" style={{ borderRadius: 999, border: "1px solid rgba(0,0,0,.08)" }}>
                      Top candidatos
                    </span>
                    <span className="badge bg-secondary" style={{ borderRadius: 999 }}>
                      {d.topCandidates?.length ?? 0}
                    </span>
                  </div>
                </div>

                {/* Table */}
                <div className="table-responsive mt-3">
                  <table className="table align-middle mb-0">
                    <thead>
                      <tr className="text-muted" style={{ fontSize: 13 }}>
                        <th>Usuario</th>
                        <th>Compatibilidad</th>
                        <th>Estado</th>
                        <th>Fecha</th>
                        <th className="text-end" style={{ width: 140 }}></th>
                      </tr>
                    </thead>
                    <tbody>
                      {d.topCandidates?.map((c) => {
                        const approved = (c.status || "").toLowerCase().includes("aprob");
                        return (
                          <tr key={c.applicationId}>
                            <td style={{ maxWidth: 360 }}>
                              <div className="fw-semibold text-truncate" title={c.userEmail}>
                                {c.userEmail}
                              </div>
                            </td>
                            <td>
                              <ScoreBadge score={c.compatibilityScore} />
                            </td>
                            <td>
                              <StatusBadge status={c.status} />
                            </td>
                            <td className="text-muted" style={{ fontSize: 13 }}>
                              {c.createdAt ? new Date(c.createdAt).toLocaleString() : "-"}
                            </td>
                            <td className="text-end">
                              <MotionDiv whileHover={{ scale: approved ? 1 : 1.02 }} whileTap={{ scale: approved ? 1 : 0.98 }}>
                                <button
                                  className={approved ? "btn btn-outline-secondary btn-sm" : "btn btn-success btn-sm"}
                                  style={{ borderRadius: 999, paddingInline: 14, fontWeight: 700 }}
                                  onClick={() => approve(c.applicationId)}
                                  disabled={approved || busyId === c.applicationId}
                                >
                                  {approved
                                    ? "Aprobada"
                                    : busyId === c.applicationId
                                    ? "Aprobando..."
                                    : "Aprobar"}
                                </button>
                              </MotionDiv>
                            </td>
                          </tr>
                        );
                      })}

                      {(!d.topCandidates || d.topCandidates.length === 0) && (
                        <tr>
                          <td colSpan="5" className="text-muted">
                            (Sin solicitudes en la ventana)
                          </td>
                        </tr>
                      )}
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          ))}
      </div>
    </div>
  );
}
