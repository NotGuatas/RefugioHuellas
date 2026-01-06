import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { getDog } from "../api/api";

export default function DogDetail({ token }) {
    const { id } = useParams();
    const [dog, setDog] = useState(null);
    const [err, setErr] = useState("");

    useEffect(() => {
        getDog(id, token)
            .then(setDog)
            .catch((e) => setErr(e.message));
    }, [id, token]);

    if (err) return <div>{err}</div>;
    if (!dog) return <div>Cargando...</div>;

    return (
        <div style={{ display: "grid", gap: 12 }}>
            <Link to="/dogs">? Volver</Link>

            <h2 style={{ margin: 0 }}>{dog.name}</h2>

            <img
                src={dog.photoUrl || "https://via.placeholder.com/600x300"}
                alt={dog.name}
                style={{ width: "100%", maxWidth: 800, borderRadius: 16 }}
            />

            <div>
                <div><b>Tama�o:</b> {dog.size}</div>
                <div><b>Energ�a:</b> {dog.energyLevel}</div>
                <div><b>Entorno ideal:</b> {dog.idealEnvironment}</div>
                <div>
                    <b>Compatibilidad:</b>{" "}
                    {dog.compatibilityScore ?? "No disponible (sin perfil o sin sesi�n)"}
                </div>
            </div>
        </div>
    );
}
