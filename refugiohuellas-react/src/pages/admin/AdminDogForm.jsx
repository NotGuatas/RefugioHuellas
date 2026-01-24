import { useEffect, useMemo, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import { adminApi } from "../../api";

export default function AdminDogForm() {
  const { token } = useAuth();
  const { id } = useParams();
  const isEdit = !!id;
  const nav = useNavigate();

  const [origins, setOrigins] = useState([]);
  const [err, setErr] = useState("");
  const [saving, setSaving] = useState(false);

  const [form, setForm] = useState({
    name: "",
    description: "",
    breed: "",
    size: "Mediano",
    energyLevel: 3,
    idealEnvironment: "",
    originTypeId: 0,
    healthStatus: "",
    sterilized: false,
    photoFile: null,
  });

  // preview foto
  const photoPreview = useMemo(() => {
    if (form.photoFile instanceof File) return URL.createObjectURL(form.photoFile);
    return "";
  }, [form.photoFile]);

  useEffect(() => {
    async function boot() {
      try {
        setErr("");
        const o = await adminApi.originTypes();
        setOrigins(Array.isArray(o) ? o : []);

        if (!isEdit) return;

        const d = await adminApi.dogGet(token, id);
        setForm((f) => ({
          ...f,
          name: d.name || "",
          description: d.description || "",
          breed: d.breed || "",
          size: d.size || "Mediano",
          energyLevel: d.energyLevel ?? 3,
          idealEnvironment: d.idealEnvironment || "",
          originTypeId: d.originTypeId ?? 0,
          healthStatus: d.healthStatus || "",
          sterilized: !!d.sterilized,
          photoFile: null, // no precargar file
        }));
      } catch (e) {
        setErr(e?.message || "Error al cargar datos");
      }
    }
    boot();
  }, [token, id, isEdit]);

  useEffect(() => {
    return () => {
      if (photoPreview) URL.revokeObjectURL(photoPreview);
    };
  }, [photoPreview]);

  function onChange(e) {
    const { name, value, type, checked, files } = e.target;

    setForm((f) => ({
      ...f,
      [name]:
        type === "checkbox"
          ? checked
          : type === "file"
          ? (files && files[0]) || null
          : name === "energyLevel"
          ? Number(value)
          : name === "originTypeId"
          ? Number(value)
          : value,
    }));
  }

  function validate() {
    const name = (form.name || "").trim();
    const breed = (form.breed || "").trim();
    const env = (form.idealEnvironment || "").trim();
    const energy = Number(form.energyLevel);

    if (!name) return "El nombre es obligatorio.";
    if (!breed) return "La raza es obligatoria.";
    if (!env) return "El entorno ideal es obligatorio.";
    if (!Number.isFinite(energy) || energy < 1 || energy > 5)
      return "La energía debe estar entre 1 y 5.";
    if (!form.originTypeId || form.originTypeId <= 0)
      return "Selecciona el origen del perro.";
    if (!isEdit && !form.photoFile) return "La foto es obligatoria al crear.";
    return "";
  }

  async function onSubmit(e) {
    e.preventDefault();
    setErr("");

    const msg = validate();
    if (msg) {
      setErr(msg);
      return;
    }

    setSaving(true);
    try {
      const fd = new FormData();
      fd.append("Name", form.name.trim());
      fd.append("Description", form.description || "");
      fd.append("Breed", form.breed.trim());
      fd.append("Size", form.size);
      fd.append("EnergyLevel", String(form.energyLevel));
      fd.append("IdealEnvironment", form.idealEnvironment.trim());
      fd.append("OriginTypeId", String(form.originTypeId));
      fd.append("HealthStatus", form.healthStatus || "");
      fd.append("Sterilized", String(form.sterilized));
      if (form.photoFile) fd.append("PhotoFile", form.photoFile);

      if (!isEdit) await adminApi.dogCreate(token, fd);
      else await adminApi.dogUpdate(token, id, fd);

      nav("/admin/dogs");
    } catch (e2) {
      setErr(e2?.message || "Error al guardar");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="container py-3" style={{ maxWidth: 980 }}>
      <div className="d-flex align-items-center justify-content-between mb-3">
        <h2 className="m-0">{isEdit ? "Editar perro" : "Nuevo perro"}</h2>
        <Link to="/admin/dogs" className="btn btn-outline-secondary btn-sm">
          ← Volver
        </Link>
      </div>

      {err && (
        <div className="alert alert-danger" role="alert">
          {err}
        </div>
      )}

      <div className="card border-0 shadow-sm" style={{ borderRadius: 16 }}>
        <div className="card-body p-4">
          <form onSubmit={onSubmit} className="row g-3">
            {/* Nombre */}
            <div className="col-12">
              <label className="form-label">Nombre</label>
              <input
                name="name"
                value={form.name}
                onChange={onChange}
                className="form-control"
                placeholder="Ej: Luna"
              />
            </div>

            {/* Descripción */}
            <div className="col-12">
              <label className="form-label">Descripción</label>
              <textarea
                name="description"
                value={form.description}
                onChange={onChange}
                rows={4}
                className="form-control"
                placeholder="Describe al perrito..."
              />
            </div>

            {/* Raza */}
            <div className="col-md-6">
              <label className="form-label">Raza</label>
              <input
                name="breed"
                value={form.breed}
                onChange={onChange}
                className="form-control"
                placeholder="Ej: Mestizo"
              />
            </div>

            {/* Tamaño */}
            <div className="col-md-6">
              <label className="form-label">Tamaño</label>
              <select
                name="size"
                value={form.size}
                onChange={onChange}
                className="form-select"
              >
                <option>Pequeño</option>
                <option>Mediano</option>
                <option>Grande</option>
              </select>
            </div>

            {/* Energía */}
            <div className="col-md-6">
              <label className="form-label">Energía (1–5)</label>
              <input
                name="energyLevel"
                type="number"
                min="1"
                max="5"
                value={form.energyLevel}
                onChange={onChange}
                className="form-control"
              />
              <div className="form-text">1 = baja, 5 = alta</div>
            </div>

            {/* Entorno ideal */}
            <div className="col-md-6">
              <label className="form-label">Entorno ideal</label>
              <input
                name="idealEnvironment"
                value={form.idealEnvironment}
                onChange={onChange}
                className="form-control"
                placeholder="Ej: Casa con patio"
              />
            </div>

            {/* Origen */}
            <div className="col-md-6">
              <label className="form-label">Origen del perro</label>
              <select
                name="originTypeId"
                value={form.originTypeId}
                onChange={onChange}
                className="form-select"
              >
                <option value={0}>-- Seleccione --</option>
                {origins.map((o) => (
                  <option key={o.id} value={o.id}>
                    {o.name}
                  </option>
                ))}
              </select>
            </div>

            {/* Salud */}
            <div className="col-md-6">
              <label className="form-label">Estado de salud</label>
              <input
                name="healthStatus"
                value={form.healthStatus}
                onChange={onChange}
                className="form-control"
                placeholder="Ej: Vacunas al día, desparasitado..."
              />
            </div>

            {/* Esterilizado */}
            <div className="col-12">
              <div className="form-check">
                <input
                  name="sterilized"
                  type="checkbox"
                  checked={form.sterilized}
                  onChange={onChange}
                  className="form-check-input"
                  id="sterilizedChk"
                />
                <label className="form-check-label" htmlFor="sterilizedChk">
                  Esterilizado
                </label>
              </div>
            </div>

            {/* Foto + preview */}
            <div className="col-12">
              <label className="form-label">
                Foto {isEdit ? "(opcional)" : "(obligatoria)"}
              </label>
              <input
                name="photoFile"
                type="file"
                accept="image/*"
                onChange={onChange}
                className="form-control"
              />

              {photoPreview && (
                <div className="mt-3 d-flex align-items-center gap-3">
                  <img
                    src={photoPreview}
                    alt="preview"
                    style={{
                      width: 120,
                      height: 90,
                      objectFit: "cover",
                      borderRadius: 12,
                      border: "1px solid rgba(0,0,0,.08)",
                      background: "#eee",
                    }}
                  />
                  <div className="text-muted small">
                    Vista previa de la foto seleccionada.
                  </div>
                </div>
              )}
            </div>

            {/* Actions */}
            <div className="col-12 d-flex gap-2 justify-content-end mt-2">
              <Link to="/admin/dogs" className="btn btn-outline-secondary">
                Cancelar
              </Link>
              <button className="btn btn-success" disabled={saving}>
                {saving ? "Guardando..." : isEdit ? "Guardar cambios" : "Crear perro"}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
