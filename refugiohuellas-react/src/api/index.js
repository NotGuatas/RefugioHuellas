import { apiFetch } from "./client";

// Exporta también las funciones sueltas (login, getDogs  por compatibilidad
export * from "./api";

export const authApi = {
  login: (email, password) =>
    apiFetch("/api/auth/login", { method: "POST", body: { email, password } }),

  register: (email, password) =>
    apiFetch("/api/auth/register", { method: "POST", body: { email, password } }),

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

export const adoptionsApi = {
  create: (token, payload) =>
    apiFetch("/api/adoptions", { method: "POST", token, body: payload }),

  my: (token) => apiFetch("/api/adoptions/me", { token }),
};

export const adminApi = {
  // catálogo
  originTypes: () => apiFetch("/api/origin-types"),

  // dogs admin
  dogsList: (token) => apiFetch("/api/admin/dogs", { token }),
  dogGet: (token, id) => apiFetch(`/api/admin/dogs/${id}`, { token }),

  dogCreate: (token, formData) =>
    apiFetch("/api/admin/dogs", { method: "POST", token, body: formData }),

  dogUpdate: (token, id, formData) =>
    apiFetch(`/api/admin/dogs/${id}`, { method: "PUT", token, body: formData }),

  dogDelete: (token, id) =>
    apiFetch(`/api/admin/dogs/${id}`, { method: "DELETE", token }),

  // adoptions admin
  adoptionsList: (token) => apiFetch("/api/admin/adoptions", { token }),
  adoptionDetail: (token, id) => apiFetch(`/api/admin/adoptions/${id}`, { token }),
  adoptionApprove: (token, id) =>
    apiFetch(`/api/admin/adoptions/${id}/approve`, { method: "POST", token }),

  // matches admin
  matchesBest: (token) => apiFetch("/api/admin/matches/best", { token }),
  matchesTop: (token, days = 7) => apiFetch(`/api/admin/matches/top?days=${days}`, { token }),
  matchApprove: (token, applicationId) =>
    apiFetch("/api/admin/matches/approve", {
      method: "POST",
      token,
      body: { applicationId },
    }),
};

