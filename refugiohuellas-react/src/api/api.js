const envBase = import.meta.env.VITE_API_BASE_URL;
const BASE_URL = (envBase && envBase.trim() !== "")
    ? envBase.replace(/^http:\/\//i, "https://")
    : window.location.origin;



async function parseJson(res) {
    const data = await res.json().catch(() => null);
    if (!res.ok) {
        throw new Error(data?.message || `Error ${res.status}`);
    }
    return data;
}

export async function login(email, password) {
    const res = await fetch(`${BASE_URL}/api/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password }),
    });
    return parseJson(res); // { token, email, userId, roles }
}

export async function me(token) {
    const res = await fetch(`${BASE_URL}/api/auth/me`, {
        headers: { Authorization: `Bearer ${token}` },
    });
    return parseJson(res);
}

export async function getDogs() {
    const res = await fetch(`${BASE_URL}/api/dogs`);
    return parseJson(res); // [{id,name,photoUrl,size,energyLevel,idealEnvironment}]
}

export async function getDog(id, token) {
    const headers = token ? { Authorization: `Bearer ${token}` } : {};
    const res = await fetch(`${BASE_URL}/api/dogs/${id}`, { headers });
    return parseJson(res); // incluye CompatibilityScore si hay token + perfil
}

export async function getTraits() {
    const res = await fetch(`${BASE_URL}/api/traits`);
    return parseJson(res);
}

export async function getMyTraits(token) {
    const res = await fetch(`${BASE_URL}/api/user-traits/me`, {
        headers: { Authorization: `Bearer ${token}` },
    });
    return parseJson(res); // [{ traitId, value }]
}

export async function saveMyTraits(token, items) {
    const res = await fetch(`${BASE_URL}/api/user-traits/me`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ items }),
    });
    return parseJson(res);
}

