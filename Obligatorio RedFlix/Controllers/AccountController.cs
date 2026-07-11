using Obligatorio_RedFlix.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace Obligatorio_RedFlix.Controllers
{
    public class AccountController : Controller
    {
        private RedFlixDBEntities db = new RedFlixDBEntities();

        public ActionResult Login()
        {
            if (Session["UsuarioId"] != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Por favor completá todos los campos.";
                return View();
            }

            string passwordHash = GenerarHash(password);

            var usuario = db.Usuarios
                .Include("Role")
                .FirstOrDefault(u =>
                    u.Email == email &&
                    u.PasswordHash == passwordHash &&
                    u.Estado == "Activo"
                );

            if (usuario == null)
            {
                ViewBag.Error = "Email o contraseña incorrectos.";
                return View();
            }

            Session["UsuarioId"] = usuario.IdUsuario;
            Session["UsuarioNombre"] = usuario.Nombre;
            Session["UsuarioEmail"] = usuario.Email;

            Session["RolId"] = usuario.IdRol;
            Session["RolNombre"] = usuario.Role.Nombre;

            // También guardamos esto con nombre más simple por si lo usamos en otras vistas
            Session["Rol"] = usuario.Role.Nombre;

            // Cargamos los permisos del rol del usuario
            var permisos = db.Database.SqlQuery<string>(
                @"SELECT p.NombrePermiso
          FROM RolesPermisos rp
          INNER JOIN Permisos p ON rp.IdPermiso = p.IdPermiso
          WHERE rp.IdRol = @p0",
                usuario.IdRol
            ).ToList();

            Session["Permisos"] = permisos;

            Session.Remove("PerfilNombre");
            Session.Remove("PerfilColor");
            Session.Remove("PerfilInicial");

            TempData["Success"] = "¡Bienvenido, " + usuario.Nombre + "!";

            return RedirectToAction("Seleccionar", "Perfiles");
        }
        public ActionResult SinPermiso()
        {
            return View();
        }
        public ActionResult Registrar()
        {
            if (Session["UsuarioId"] != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(string nombre, string apellido, string email, string password, string confirmarPassword)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Por favor completá todos los campos obligatorios.";
                return View();
            }

            if (password != confirmarPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.Error = "La contraseña debe tener al menos 6 caracteres.";
                return View();
            }

            bool emailExiste = db.Usuarios.Any(u => u.Email == email);

            if (emailExiste)
            {
                ViewBag.Error = "Ya existe una cuenta con ese email.";
                return View();
            }

            var rolUsuario = db.Roles.FirstOrDefault(r => r.Nombre == "Usuario");

            if (rolUsuario == null)
            {
                ViewBag.Error = "No existe el rol Usuario en la base de datos.";
                return View();
            }

            var nuevoUsuario = new Usuario
            {
                Nombre = nombre,
                Apellido = apellido,
                Email = email,
                PasswordHash = GenerarHash(password),
                Estado = "Activo",
                FechaRegistro = DateTime.Now,
                IdRol = rolUsuario.IdRol
            };

            db.Usuarios.Add(nuevoUsuario);
            db.SaveChanges();

            TempData["Success"] = "¡Cuenta creada con éxito! Ya podés iniciar sesión.";
            return RedirectToAction("Login");
        }
        public ActionResult Logout()
        {
            Session.Clear();
            TempData["Success"] = "Cerraste sesión correctamente.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Perfil()
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Login");

            int id = (int)Session["UsuarioId"];
            var usuario = db.Usuarios.Find(id);

            if (usuario == null)
                return HttpNotFound();

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Perfil(string nombre, string apellido, string passwordActual, string passwordNuevo, string confirmarNuevo)
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Login");

            int id = (int)Session["UsuarioId"];
            var usuario = db.Usuarios.Find(id);

            if (usuario == null)
                return HttpNotFound();

            usuario.Nombre = nombre;
            usuario.Apellido = apellido;

            if (!string.IsNullOrWhiteSpace(passwordNuevo))
            {
                if (string.IsNullOrWhiteSpace(passwordActual))
                {
                    ViewBag.Error = "Debés ingresar la contraseña actual.";
                    return View(usuario);
                }

                if (usuario.PasswordHash != GenerarHash(passwordActual))
                {
                    ViewBag.Error = "La contraseña actual es incorrecta.";
                    return View(usuario);
                }

                if (passwordNuevo != confirmarNuevo)
                {
                    ViewBag.Error = "Las contraseñas nuevas no coinciden.";
                    return View(usuario);
                }

                usuario.PasswordHash = GenerarHash(passwordNuevo);
            }

            db.SaveChanges();

            Session["UsuarioNombre"] = usuario.Nombre;

            TempData["Success"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Perfil");
        }

        private string GenerarHash(string texto)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(texto);
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder resultado = new StringBuilder();

                foreach (byte b in hash)
                {
                    resultado.Append(b.ToString("x2"));
                }

                return resultado.ToString();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
