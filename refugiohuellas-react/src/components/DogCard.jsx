import { Link } from "react-router-dom";

export default function DogCard({ dog }) {
    return (
        <div
            style={{
                border: "1px solid #ddd",
                borderRadius: 14,
                padding: 12,
                display: "flex",
                gap: 12,
                alignItems: "center",
            }}
        >
            <img
                src={dog.photoUrl || "https://via.placeholder.com/100"}
                alt={dog.name}
                width="100"
                height="100"
                style={{ objectFit: "cover", borderRadius: 12 }}
            />

            <div style={{ flex: 1 }}>
                <h3 style={{ margin: 0 }}>{dog.name}</h3>
                <div style={{ fontSize: 14, color: "#444" }}>
                    <div>Tamaño: {dog.size}</div>
                    <div>Energía: {dog.energyLevel}</div>
                    <div>Entorno: {dog.idealEnvironment}</div>
                </div>
            </div>

            <Link to={`/dogs/${dog.id}`}>Ver</Link>
        </div>
    );
}
