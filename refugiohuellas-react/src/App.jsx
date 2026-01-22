import { Routes, Route, Navigate } from "react-router-dom";
import Navbar from "./components/Navbar";

import Login from "./pages/Login";
import Dogs from "./pages/Dogs";
import DogDetail from "./pages/DogDetail";
import Profile from "./pages/Profile";
import Adopt from "./pages/Adopt";
import MyApplications from "./pages/MyApplications";
import MyBestMatches from "./pages/MyBestMatches";

import RequireAuth from "./auth/RequireAuth";
import RequireAdmin from "./auth/RequireAdmin";
import { useAuth } from "./auth/AuthContext";

import AdminDogs from "./pages/admin/AdminDogs";
import AdminDogForm from "./pages/admin/AdminDogForm";
import AdminDogDelete from "./pages/admin/AdminDogDelete";
import AdminAdoptions from "./pages/admin/AdminAdoptions";
import AdminAdoptionDetail from "./pages/admin/AdminAdoptionDetail";
import AdminBestMatches from "./pages/admin/AdminBestMatches";
import AdminTopMatches from "./pages/admin/AdminTopMatches";

export default function App() {
  const auth = useAuth();


  if (!auth) {
    return (
      <div className="container py-4">
        Error: AuthContext no inicializado. Revisa que main.jsx envuelva con AuthProvider.
      </div>
    );
  }

  const { user, token, logout, isAuth, loading } = auth;

  if (loading) return <div className="container py-4">Cargando...</div>;

  return (
    <div>
      {isAuth && <Navbar user={user} onLogout={logout} />}

      <div className="container rh-page">
        <Routes>
          <Route
            path="/login"
            element={isAuth ? <Navigate to="/dogs" replace /> : <Login />}
          />

          <Route
            path="/dogs"
            element={
              <RequireAuth>
                <Dogs token={token} />
              </RequireAuth>
            }
          />

          <Route
            path="/dogs/:id"
            element={
              <RequireAuth>
                <DogDetail token={token} />
              </RequireAuth>
            }
          />

          <Route
            path="/profile"
            element={
              <RequireAuth>
                <Profile token={token} />
              </RequireAuth>
            }
          />

          <Route
            path="/adopt/:dogId"
            element={
              <RequireAuth>
                <Adopt token={token} />
              </RequireAuth>
            }
          />

          <Route
            path="/my-applications"
            element={
              <RequireAuth>
                <MyApplications token={token} />
              </RequireAuth>
            }
          />

          <Route
            path="/my-best-matches"
            element={
            <RequireAuth>
              <MyBestMatches token={token} />
            </RequireAuth>
      }
          />
          {/* ADMIN */}
          <Route
            path="/admin/dogs"
            element={
              <RequireAdmin>
                <AdminDogs token={token} />
              </RequireAdmin>
            }
          />
          <Route
            path="/admin/dogs/new"
            element={
              <RequireAdmin>
                <AdminDogForm token={token} />
              </RequireAdmin>
            }
          />
          <Route
            path="/admin/dogs/:id/edit"
            element={
              <RequireAdmin>
                <AdminDogForm token={token} />
              </RequireAdmin>
            }
          />
          <Route
            path="/admin/dogs/:id/delete"
            element={
              <RequireAdmin>
                <AdminDogDelete token={token} />
              </RequireAdmin>
            }
          />
          <Route
            path="/admin/adoptions"
            element={
              <RequireAdmin>
                <AdminAdoptions token={token} />
              </RequireAdmin>
            }
          />
          <Route
            path="/admin/adoptions/:id"
            element={
              <RequireAdmin>
                <AdminAdoptionDetail token={token} />
              </RequireAdmin>
            }
          />
          <Route
            path="/admin/matches/best"
            element={
              <RequireAdmin>
                <AdminBestMatches token={token} />
              </RequireAdmin>
            }
          />
          <Route
            path="/admin/matches/top"
            element={
              <RequireAdmin>
                <AdminTopMatches token={token} />
              </RequireAdmin>
            }
          />

          <Route
            path="/"
            element={<Navigate to={isAuth ? "/dogs" : "/login"} replace />}
          />
          <Route
            path="*"
            element={<Navigate to={isAuth ? "/dogs" : "/login"} replace />}
          />
        </Routes>

        <div className="rh-footer">
          © 2026 - RefugioHuellas &nbsp;&nbsp;|&nbsp;&nbsp; Hecho con 💚 para los peluditos
        </div>
      </div>
    </div>
  );
}
