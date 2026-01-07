import { apiFetch } from "./client";

export const authApi = {
    login: (email, password) =>
        apiFetch("/api/auth/login", { method: "POST", body: { email, password } }),

    me: (token) => apiFetch("/api/auth/me", { token }),
};

export const dogsApi = {
    list: () => apiFetch("/api/dogs"),
    get: (id, token) => apiFetch(`/api/dogs/${id}`, { token }),
};

export const traitsApi = {
    list: () => apiFetch("/api/traits"),
    my: (token) => apiFetch("/api/user-traits/me", { token }),
    saveMy: (token, items) =>
        apiFetch("/api/user-traits/me", { method: "POST", token, body: { items } }),
};
