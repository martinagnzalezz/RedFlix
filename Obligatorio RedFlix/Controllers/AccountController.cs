using Obligatorio_RedFlix.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Por favor completá todos los campos.";
                return View();
            }

            var usuario = db.Usuarios
                            .Include("Role")
                            .FirstOrDefault(u => u.Email == email
                                              && u.PasswordHash == password
                                              && u.Estado == "Activo");

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

            TempData["Success"] = "¡Bienvenido, " + usuario.Nombre + "!";
            return RedirectToAction("Index", "Home");
        }

       
        public ActionResult Registrar()
        {
            if (Session["UsuarioId"] != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        
        [HttpPost]
        public ActionResult Registrar(string nombre, string apellido,
                                     string email, string password,
                                     string confirmarPassword)
        {
            
            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password))
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

            
            var nuevoUsuario = new Usuario
            {
                Nombre = nombre,
                Apellido = apellido,
                Email = email,
                PasswordHash = password,   
                Estado = "Activo",
                FechaRegistro = DateTime.Now,
                IdRol = 2          
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
        public ActionResult Perfil(string nombre, string apellido, string passwordActual,
                                   string passwordNuevo, string confirmarNuevo)
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Login");

            int id = (int)Session["UsuarioId"];
            var usuario = db.Usuarios.Find(id);

            
            usuario.Nombre = nombre;
            usuario.Apellido = apellido;

            
            if (!string.IsNullOrEmpty(passwordNuevo))
            {
                if (usuario.PasswordHash != passwordActual)
                {
                    ViewBag.Error = "La contraseña actual es incorrecta.";
                    return View(usuario);
                }
                if (passwordNuevo != confirmarNuevo)
                {
                    ViewBag.Error = "Las contraseñas nuevas no coinciden.";
                    return View(usuario);
                }
                usuario.PasswordHash = passwordNuevo;
            }

            db.SaveChanges();

            
            Session["UsuarioNombre"] = usuario.Nombre;

            TempData["Success"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Perfil");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}