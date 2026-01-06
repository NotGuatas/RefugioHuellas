import { useEffect, useState } from "react";
import { getDogs } from "../api/api";
import DogCard from "../components/DogCard";

export default function Dogs() {
    const [dogs, setDogs] = useState([]);
    const [err, setErr] = useState("");

    useEffect(() => {
        getDogs()
            .then(setDogs)
            .catch((e) => setErr(e.message));
    }, []);

    return (
        <div>
            <h2>Perros disponibles</h2>

            {err && (
                <div style={{ background: "#ffd9d9", padding: 10, borderRadius: 10 }}>
                    {err}
                </div>
            )}

            <div style={{ display: "grid", gap: 12, marginTop: 12 }}>
                {dogs.map((d) => (
                    <DogCard key={d.id} dog={d} />
                ))}
            </div>
        </div>
    );
}
