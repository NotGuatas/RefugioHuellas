import { useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { getMyBestMatches } from "../api/api";
import { motion, AnimatePresence } from "framer-motion";

const container = {
  hidden: { opacity: 0 },
  show: {
    opacity: 1,
    transition: { staggerChildren: 0.08, delayChildren: 0.05 },
  },
};

const item = {
  hidden: { opacity: 0, y: 10, scale: 0.98 },
  show: {
    opacity: 1,
    y: 0,
    scale: 1,
    transition: { duration: 0.22 },
  },
};

function ScoreBadge({ score }) {
  const cls =
    score >= 70
      ? "bg-success"
      : score >= 50
      ? "bg-warning text-dark"
      : "bg-danger";
  return <span className={`badge ${cls}`}>{score}%</span>;
}

function SkeletonCard() {
  return (
    <div className="col-md-4">
      <div className="card h-100 rh-card">
        <div className="rh-skeleton" style={{ height: 220 }} />
        <div className="card-body" style={{ display: "grid", gap: 10 }}>
          <div className="rh-skeleton" style={{ height: 18, width: "60%" }} />
          <div className="rh-skeleton" style={{ height: 14, width: "80%" }} />
          <div className="rh-skeleton" style={{ height: 14, width: "75%" }} />
        </div>
        <div className="card-footer d-flex gap-2">
          <div className="rh-skeleton" style={{ height: 32, width: 90 }} />
          <div className="rh-skeleton" style={{ height: 32, width: 80 }} />
        </div>
      </div>
    </div>
  );
}

export default function MyBestMatches({ token }) {
  const [items, setItems] = useState([]);
  const [err, setErr] = useState("");
  const [loading, setLoading] = useState(false);
  const [revealed, setRevealed] = useState(false);

  const MotionDiv = motion.div;
  const MotionButton = motion.button;

  const canReveal = useMemo(() => !!token, [token]);

  async function reveal() {
    try {
      setErr("");
      setLoading(true);
      const data = await getMyBestMatches(token);
      setItems(Array.isArray(data) ? data : []);
      setRevealed(true);
    } catch (e) {
      setErr(e?.message || "Error al cargar coincidencias");
    } finally {
      setLoading(false);
    }
  }

  function hide() {
    setRevealed(false);
    setItems([]);
    setErr("");
  }

  if (err) return <div className="alert alert-danger">{err}</div>;

  return (
    <div style={{ maxWidth: 1100, width: "100%", margin: "0 auto" }}>
      <MotionDiv
        initial={{ opacity: 0, y: 8 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.22 }}
      >
        <h2>Mis mejores coincidencias</h2>
        <p style={{ color: "#666" }}>
          Presiona el botón para revelar tus 3 mejores matches.
        </p>
      </MotionDiv>

      {/* Panel inicial */}
      <AnimatePresence mode="wait">
        {!revealed && (
          <MotionDiv
            key="panel"
            className="card p-4 rh-card"
            initial={{ opacity: 0, y: 8, scale: 0.99 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            exit={{ opacity: 0, y: 8, scale: 0.99 }}
            transition={{ duration: 0.2 }}
          >
            <div className="d-flex align-items-center justify-content-between flex-wrap gap-2">
              <div>
                <div className="fw-bold">¿Listo para ver tus coincidencias?</div>
                <div style={{ color: "#666" }}>
                  Calcularemos tu compatibilidad y te mostraremos los mejores.
                </div>
              </div>

              <MotionButton
                className="btn btn-success"
                onClick={reveal}
                disabled={!canReveal || loading}
                whileHover={{ scale: 1.02 }}
                whileTap={{ scale: 0.98 }}
              >
                {loading ? "Calculando..." : "Revelar coincidencias"}
              </MotionButton>
            </div>
          </MotionDiv>
        )}
      </AnimatePresence>

      {/* Skeleton */}
      {loading && (
        <div className="row g-3 mt-2">
          <SkeletonCard />
          <SkeletonCard />
          <SkeletonCard />
        </div>
      )}

      {/* Cards */}
      <AnimatePresence>
        {revealed && !loading && (
          <MotionDiv
            key="grid"
            className="row g-3 mt-2"
            variants={container}
            initial="hidden"
            animate="show"
            exit={{ opacity: 0 }}
          >
            {items.map((x) => (
              <MotionDiv className="col-md-4" key={x.dogId} variants={item}>
                <div className="card h-100 rh-card">
                  {/* CONTENEDOR DE IMAGEN CENTRADA */}
                  <div
                    style={{
                      height: 220,
                      display: "flex",
                      alignItems: "center",
                      justifyContent: "center",
                      overflow: "hidden",
                      borderTopLeftRadius: 12,
                      borderTopRightRadius: 12,
                    }}
                  >
                    <img
                      src={x.photoUrl || "https://via.placeholder.com/600x300"}
                      alt={x.dogName}
                      style={{
                        width: "100%",
                        height: "100%",
                        objectFit: "cover",
                        objectPosition: "center",
                      }}
                    />
                  </div>

                  <div className="card-body">
                    <div className="d-flex align-items-center justify-content-between">
                      <h5 className="card-title mb-0">{x.dogName}</h5>
                      <ScoreBadge score={x.score} />
                    </div>

                    <div className="mt-2">
                      <div>
                        <b>Tamaño:</b> {x.sizeLabel}
                      </div>
                      <div>
                        <b>Nivel de energía:</b> {x.energy}/5
                      </div>
                    </div>
                  </div>

                  <div className="card-footer d-flex gap-2">
                    <MotionDiv whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                      <Link
                        className="btn btn-outline-primary btn-sm"
                        to={`/dogs/${x.dogId}`}
                      >
                        Ver detalle
                      </Link>
                    </MotionDiv>

                    <MotionDiv whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                      <Link
                        className="btn btn-success btn-sm"
                        to={`/adopt/${x.dogId}`}
                      >
                        Adoptar
                      </Link>
                    </MotionDiv>
                  </div>
                </div>
              </MotionDiv>
            ))}

            {items.length === 0 && (
              <MotionDiv className="col-12" variants={item}>
                <div className="alert alert-warning mt-1">
                  Aún no hay coincidencias. Completa tu perfil para generar
                  compatibilidad.
                  <div className="mt-2">
                    <Link to="/profile" className="btn btn-primary btn-sm">
                      Ir a Mi perfil
                    </Link>
                  </div>
                </div>
              </MotionDiv>
            )}

            <MotionDiv className="col-12" variants={item}>
              <button
                className="btn btn-outline-secondary btn-sm mt-2"
                onClick={hide}
              >
                Ocultar coincidencias
              </button>
            </MotionDiv>
          </MotionDiv>
        )}
      </AnimatePresence>
    </div>
  );
}
