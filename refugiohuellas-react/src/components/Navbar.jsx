import { Link, NavLink } from "react-router-dom";

export default function Navbar({ user, onLogout }) {
  const email = user?.email || "";

  const isAdmin =
    (user?.role || "").toLowerCase() === "admin" ||
    (Array.isArray(user?.roles) && user.roles.includes("Admin")) ||
    user?.isAdmin === true;

  const linkClass = ({ isActive }) =>
    "nav-link px-2 " + (isActive ? "text-white fw-bold" : "text-white-50");

  return (
    <nav className="navbar navbar-expand-lg navbar-dark bg-success">
      <div className="container">
        <Link className="navbar-brand fw-bold" to="/dogs">
          RefugioHuellas
        </Link>

        <button
          className="navbar-toggler"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#rhNavbar"
          aria-controls="rhNavbar"
          aria-expanded="false"
          aria-label="Toggle navigation"
        >
          <span className="navbar-toggler-icon" />
        </button>

        <div className="collapse navbar-collapse" id="rhNavbar">
          <ul className="navbar-nav me-auto mb-2 mb-lg-0">
            <li className="nav-item">
              <NavLink className={linkClass} to="/dogs">
                Perros
              </NavLink>
            </li>

            <li className="nav-item">
              <NavLink className={linkClass} to="/my-applications">
                Mis solicitudes
              </NavLink>
            </li>

            <li className="nav-item">
              <NavLink className={linkClass} to="/my-best-matches">
                Mis mejores coincidencias
              </NavLink>
            </li>

            {isAdmin && (
              <>
                <li className="nav-item">
                  <NavLink className={linkClass} to="/admin/dogs">
                    Perros (Admin)
                  </NavLink>
                </li>

                <li className="nav-item">
                  <NavLink className={linkClass} to="/admin/adoptions">
                    Solicitudes (Admin)
                  </NavLink>
                </li>

                <li className="nav-item">
                  <NavLink className={linkClass} to="/admin/matches/best">
                    Mejores coincidencias
                  </NavLink>
                </li>

                <li className="nav-item">
                  <NavLink className={linkClass} to="/admin/matches/top">
                    Mejores candidatos
                  </NavLink>
                </li>
              </>
            )}
          </ul>

          <div className="d-flex align-items-center gap-3">
            {email ? (
              <Link className="text-white text-decoration-none" to="/profile">
                Mi perfil ({email})
              </Link>
            ) : (
              <span className="text-white-50">No autenticado</span>
            )}

            <button className="btn btn-outline-light btn-sm" onClick={onLogout}>
              Salir
            </button>
          </div>
        </div>
      </div>
    </nav>
  );
}
