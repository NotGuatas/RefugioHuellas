import { Link } from "react-router-dom";
import { motion } from "framer-motion";

export default function DogCard({ dog }) {
  const MotionDiv = motion.div;

  // Soporta ambos modelos de datos:
  // - Nuevo: sizeLabel / energy / environmentLabel
  const sizeText = dog.sizeLabel ?? dog.size ?? "-";
  const energyText = dog.energy ?? dog.energyLevel ?? "-";
  const envText = dog.environmentLabel ?? dog.idealEnvironment ?? "-";

  return (
    <div className="card border-0 shadow-sm" style={{ borderRadius: 16 }}>
      <div className="card-body d-flex align-items-center justify-content-between gap-3">
        {/* Imagen + info */}
        <div className="d-flex align-items-center gap-3">
          <img
            src={dog.photoUrl || "https://via.placeholder.com/100"}
            alt={dog.name}
            width={100}
            height={100}
            style={{
              width: 80,
              height: 80,
              objectFit: "cover",
              borderRadius: 14,
              background: "#eee",
            }}
          />

          <div>
            <div className="fw-bold fs-5">{dog.name}</div>

            {/* aquí vuelven a verse los valores */}
            <div className="text-muted small" style={{ lineHeight: 1.35 }}>
              <div>
                <b>Tamaño:</b> {sizeText}
              </div>
              <div>
                <b>Energía:</b> {energyText}
              </div>
              <div>
                <b>Entorno:</b> {envText}
              </div>
            </div>
          </div>
        </div>

        {/* Botón VER */}
        <MotionDiv whileHover={{ scale: 1.05 }} whileTap={{ scale: 0.95 }}>
          <Link
            to={`/dogs/${dog.id}`}
            className="btn btn-outline-primary btn-sm d-flex align-items-center gap-1"
            style={{
              borderRadius: 999,
              paddingInline: 14,
              fontWeight: 600,
            }}
          >
            Ver
          </Link>
        </MotionDiv>
      </div>
    </div>
  );
}
