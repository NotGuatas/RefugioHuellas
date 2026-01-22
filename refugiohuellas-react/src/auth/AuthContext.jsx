import { createContext, useContext, useEffect, useMemo, useState } from "react";
import { authApi } from "../api";

const AuthCtx = createContext(null);

export function AuthProvider({ children }) {
    const [token, setToken] = useState(() => localStorage.getItem("token") || "");
    const [user, setUser] = useState(() => {
        const raw = localStorage.getItem("user");
        return raw ? JSON.parse(raw) : null;
    });
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let alive = true;

        async function boot() {
            try {
                if (!token) {
                    if (alive) setUser(null);
                    return;
                }
                const me = await authApi.me(token);
                if (!alive) return;
                setUser(me);
                localStorage.setItem("user", JSON.stringify(me));
            } catch {
                // token invalido/expirado
                setToken("");
                setUser(null);
                localStorage.removeItem("token");
                localStorage.removeItem("user");
            } finally {
                if (alive) setLoading(false);
            }
        }

        boot();
        return () => {
            alive = false;
        };
    }, [token]);

    async function login(email, password) {
        const res = await authApi.login(email, password); // { token, email, userId }
        setToken(res.token);
        localStorage.setItem("token", res.token);

        // pedir /me para roles
        const me = await authApi.me(res.token);
        setUser(me);
        localStorage.setItem("user", JSON.stringify(me));
    }

    function logout() {
        setToken("");
        setUser(null);
        localStorage.removeItem("token");
        localStorage.removeItem("user");
    }

    const value = useMemo(
        () => ({ token, user, loading, login, logout, isAuth: !!token }),
        [token, user, loading]
    );

    return <AuthCtx.Provider value={value}>{children}</AuthCtx.Provider>;
}
// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
    return useContext(AuthCtx);
}
