import { useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import { adminApi } from "../../api";

function StatusBadge({ value }) {
  const v = (value || "").toLowerCase();
  const cls =
    v.includes("aprob") ? "text-bg-success" : v.includes("rech") ? "text-bg-danger" : "text-bg-warning";
  return <span className={`badge ${cls}`}>{value || "-"}</span>;
}

function ScoreBadge({ value }) {
  const n = Number(value ?? 0);
  const cls = n >= 70 ? "text-bg-success" : n >= 50 ? "text-bg-warning" : "text-bg-danger";
  return <span className={`badge ${cls}`}>{Number.isFinite(n) ? `${n}%` : "-"}</span>;
}

export default function AdminAdoptionDetail() {
  const { token } = useAuth();
  const { id } = useParams();

  const [data, setData] = useState(null);
  const [err, setErr] = useState("");
  const [ok, setOk] = useState("");
  const [loading, setLoading] = useState(true);
  const [approving, setApproving] = useState(false);

  useEffect(() => {
    let alive = true;

    (async () => {
      try {
        setErr("");
        setOk("");
        setLoading(true);
        const d = await adminApi.adoptionDetail(token, id);
        if (!alive) return;
        setData(d);
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
  }, [token, id]);

  const createdAtText = useMemo(() => {
    if (!data?.createdAt) return "-";
    const d = new Date(data.createdAt);
    return Number.isNaN(d.getTime()) ? String(data.createdAt) : d.toLocaleString();
  }, [data]);

  async function approve() {
    setErr("");
    setOk("");
    setApproving(true);
    try {
      const res = await adminApi.adoptionApprove(token, id);
      setOk(res?.message || "Solicitud aprobada.");

      const fresh = await adminApi.adoptionDetail(token, id);
      setData(fresh);
    } catch (e) {
      setErr(e?.message || "Error al aprobar");
    } finally {
      setApproving(false);
    }
  }

  return (
    <div className="container py-3" style={{ maxWidth: 980 }}>
      <div className="d-flex align-items-center justify-content-between mb-3">
        <Link to="/admin/adoptions" className="btn btn-outline-secondary btn-sm">
          ← Volver
        </Link>

        <div className="text-muted small">Solicitud #{id}</div>
      </div>

      <h2 className="mb-3">Detalle de solicitud</h2>

      {err && (
        <div className="alert alert-danger" role="alert">
          {err}
        </div>
      )}
      {ok && (
        <div className="alert alert-success" role="alert">
          {ok}
        </div>
      )}

      {loading ? (
        <div className="text-muted">Cargando...</div>
      ) : !data ? (
        <div className="text-muted">No se encontró la solicitud.</div>
      ) : (
        <>
          <div className="card border-0 shadow-sm mb-3" style={{ borderRadius: 16 }}>
            <div className="card-body p-4">
              <div className="d-flex align-items-start justify-content-between gap-3 flex-wrap">
                <div>
                  <div className="text-muted small mb-1">Usuario</div>
                  <div className="fw-semibold">{data.userEmail || data.userId || "-"}</div>

                  <div className="text-muted small mt-3 mb-1">Perro</div>
                  <div className="fw-semibold">{data.dogName || "-"}</div>
                </div>

                <div className="d-flex gap-2 align-items-center">
                  <div className="text-muted small">Estado:</div>
                  <StatusBadge value={data.status} />
                </div>
              </div>

              <hr className="my-3" />

              <div className="row g-3">
                <div className="col-md-6">
                  <div className="border rounded p-3">
                    <div className="text-muted small">Teléfono</div>
                    <div className="fw-semibold">{data.phone || "-"}</div>
                  </div>
                </div>

                <div className="col-md-6">
                  <div className="border rounded p-3">
                    <div className="text-muted small">Compatibilidad</div>
                    <div className="d-flex align-items-center gap-2">
                      <ScoreBadge value={data.compatibilityScore} />
                      <span className="text-muted small">según perfil</span>
                    </div>
                  </div>
                </div>

                <div className="col-12">
                  <div className="border rounded p-3">
                    <div className="text-muted small">Fecha</div>
                    <div className="fw-semibold">{createdAtText}</div>
                  </div>
                </div>
              </div>

              <div className="d-flex justify-content-end mt-3">
                <button
                  className="btn btn-success"
                  onClick={approve}
                  disabled={approving || data.status === "Aprobada"}
                >
                  {data.status === "Aprobada"
                    ? "Aprobada"
                    : approving
                    ? "Aprobando..."
                    : "Aprobar"}
                </button>
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
