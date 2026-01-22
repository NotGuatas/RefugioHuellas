import { useEffect, useState } from "react";
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

  useEffect(() => {
    async function boot() {
      try {
        const o = await adminApi.originTypes();
        setOrigins(o);

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
          photoFile: null,
        }));
      } catch (e) {
        setErr(e.message);
      }
    }
    boot();
  }, [token, id, isEdit]);

  function onChange(e) {
    const { name, value, type, checked, files } = e.target;
    setForm((f) => ({
      ...f,
      [name]:
        type === "checkbox"
          ? checked
          : type === "file"
          ? (files && files[0]) || null
          : value,
    }));
  }

  async function onSubmit(e) {
    e.preventDefault();
    setErr("");
    setSaving(true);

    try {
      const fd = new FormData();
      fd.append("Name", form.name);
      fd.append("Description", form.description);
      fd.append("Breed", form.breed);
      fd.append("Size", form.size);
      fd.append("EnergyLevel", String(form.energyLevel));
      fd.append("IdealEnvironment", form.idealEnvironment);
      fd.append("OriginTypeId", String(form.originTypeId));
      fd.append("HealthStatus", form.healthStatus);
      fd.append("Sterilized", String(form.sterilized));
      if (form.photoFile) fd.append("PhotoFile", form.photoFile);

      if (!isEdit) {
        await adminApi.dogCreate(token, fd);
      } else {
        await adminApi.dogUpdate(token, id, fd);
      }

      nav("/admin/dogs");
    } catch (e2) {
      setErr(e2.message);
    } finally {
      setSaving(false);
    }
  }

  return (
    <div style={{ display: "grid", gap: 12, maxWidth: 700 }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
        <h2 style={{ margin: 0 }}>{isEdit ? "Editar perro" : "Nuevo perro"}</h2>
        <Link to="/admin/dogs">← Volver</Link>
      </div>

      {err && <div style={{ color: "crimson" }}>{err}</div>}

      <form onSubmit={onSubmit} style={{ display: "grid", gap: 10 }}>
        <label>
          Nombre
          <input name="name" value={form.name} onChange={onChange} style={{ width: "100%" }} />
        </label>

        <label>
          Descripción
          <textarea
            name="description"
            value={form.description}
            onChange={onChange}
            rows={4}
            style={{ width: "100%" }}
          />
        </label>

        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 10 }}>
          <label>
            Raza
            <input name="breed" value={form.breed} onChange={onChange} style={{ width: "100%" }} />
          </label>

          <label>
            Tamaño
            <select name="size" value={form.size} onChange={onChange} style={{ width: "100%" }}>
              <option>Pequeño</option>
              <option>Mediano</option>
              <option>Grande</option>
            </select>
          </label>

          <label>
            Energía (1-5)
            <input
              name="energyLevel"
              type="number"
              min="1"
              max="5"
              value={form.energyLevel}
              onChange={onChange}
              style={{ width: "100%" }}
            />
          </label>

          <label>
            Entorno ideal
            <input
              name="idealEnvironment"
              value={form.idealEnvironment}
              onChange={onChange}
              style={{ width: "100%" }}
            />
          </label>

          <label>
            Origen del perro
            <select
              name="originTypeId"
              value={form.originTypeId}
              onChange={onChange}
              style={{ width: "100%" }}
            >
              <option value={0}>-- Seleccione --</option>
              {origins.map((o) => (
                <option key={o.id} value={o.id}>
                  {o.name}
                </option>
              ))}
            </select>
          </label>

          <label>
            Estado de salud
            <input
              name="healthStatus"
              value={form.healthStatus}
              onChange={onChange}
              style={{ width: "100%" }}
            />
          </label>
        </div>

        <label style={{ display: "flex", gap: 8, alignItems: "center" }}>
          <input name="sterilized" type="checkbox" checked={form.sterilized} onChange={onChange} />
          Esterilizado
        </label>

        <label>
          Foto {isEdit ? "(opcional)" : "(obligatoria)"}
          <input name="photoFile" type="file" accept="image/*" onChange={onChange} />
        </label>

        <button disabled={saving}>
          {saving ? "Guardando..." : isEdit ? "Guardar cambios" : "Crear perro"}
        </button>
      </form>
    </div>
  );
}
