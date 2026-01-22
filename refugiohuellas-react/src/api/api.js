import { apiFetch } from "./client";

// Estas funciones quedan por compatibilidad si algún componente las usa 

export function login(email, password) {
  return apiFetch("/api/auth/login", {
    method: "POST",
    body: { email, password },
  });
}

export function me(token) {
  return apiFetch("/api/auth/me", { token });
}

export function getDogs() {
  return apiFetch("/api/dogs");
}

export function getDog(id, token) {
  return apiFetch(`/api/dogs/${id}`, { token });
}

export function getTraits() {
  return apiFetch("/api/traits");
}

export function getMyTraits(token) {
  return apiFetch("/api/user-traits/me", { token });
}

export function getMyBestMatches(token) {
  return apiFetch("/api/adoptionapplications/my-best-matches", { token });
}



export function saveMyTraits(token, items) {
  return apiFetch("/api/user-traits/me", {
    method: "POST",
    token,
    body: { items },
  });
}
