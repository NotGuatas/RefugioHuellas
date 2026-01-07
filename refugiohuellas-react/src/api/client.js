const envBase = import.meta.env.VITE_API_BASE_URL;

export const BASE_URL =
    envBase && envBase.trim() !== ""
        ? envBase.replace(/\/$/, "")
        : window.location.origin;

async function parseJson(res) {
    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        const msg = data?.message || data || `Error ${res.status}`;
        throw new Error(typeof msg === "string" ? msg : JSON.stringify(msg));
    }
    return data;
}

export async function apiFetch(path, { token, ...options } = {}) {
    const headers = {
        ...(options.headers || {}),
        Accept: "application/json",
    };

    if (!(options.body instanceof FormData)) {
        // si vas a mandar JSON
        if (options.body && typeof options.body !== "string") {
            headers["Content-Type"] = "application/json";
            options.body = JSON.stringify(options.body);
        } else if (options.body && typeof options.body === "string") {
            headers["Content-Type"] = "application/json";
        }
    }

    if (token) headers.Authorization = `Bearer ${token}`;

    const res = await fetch(`${BASE_URL}${path}`, { ...options, headers });
    return parseJson(res);
}
