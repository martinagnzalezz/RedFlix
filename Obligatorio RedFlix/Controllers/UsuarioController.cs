using Obligatorio_RedFlix.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace Obligatorio_RedFlix.Controllers
{
    public class UsuariosController : Controller
    {
        private RedFlixDBEntities db = new RedFlixDBEntities();

        // LISTAR USUARIOS
        public ActionResult Index()
        {
            var usuarios = db.Usuarios.ToList();
            return View(usuarios);
        }

        // CREAR USUARIO - GET
        public ActionResult Create()
        {
            ViewBag.IdRol = new SelectList(db.Roles, "IdRol", "Nombre");
            return View();
        }

        // CREAR USUARIO - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Usuario usuario, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "La contraseña es obligatoria.");
            }

            bool emailExiste = db.Usuarios.Any(u => u.Email == usuario.Email);

            if (emailExiste)
            {
                ModelState.AddModelError("Email", "Ya existe un usuario con ese email.");
            }

            if (ModelState.IsValid)
            {
                usuario.PasswordHash = GenerarHash(password);
                usuario.FechaRegistro = DateTime.Now;
                usuario.Estado = "Activo";

                db.Usuarios.Add(usuario);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.IdRol = new SelectList(db.Roles, "IdRol", "Nombre", usuario.IdRol);
            return View(usuario);
        }

        // EDITAR USUARIO - GET
        public ActionResult Edit(int id)
        {
            Usuario usuario = db.Usuarios.Find(id);

            if (usuario == null)
            {
                return HttpNotFound();
            }

            ViewBag.IdRol = new SelectList(db.Roles, "IdRol", "Nombre", usuario.IdRol);
            return View(usuario);
        }

        // EDITAR USUARIO - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Usuario usuario)
        {
            Usuario usuarioBD = db.Usuarios.Find(usuario.IdUsuario);

            if (usuarioBD == null)
            {
                return HttpNotFound();
            }

            bool emailExiste = db.Usuarios.Any(u => u.Email == usuario.Email && u.IdUsuario != usuario.IdUsuario);

            if (emailExiste)
            {
                ModelState.AddModelError("Email", "Ya existe otro usuario con ese email.");
            }

            ModelState.Remove("PasswordHash");
            ModelState.Remove("FechaRegistro");
            ModelState.Remove("Estado");

            if (ModelState.IsValid)
            {
                usuarioBD.Nombre = usuario.Nombre;
                usuarioBD.Apellido = usuario.Apellido;
                usuarioBD.Email = usuario.Email;
                usuarioBD.IdRol = usuario.IdRol;

                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.IdRol = new SelectList(db.Roles, "IdRol", "Nombre", usuario.IdRol);
            return View(usuario);
        }

        // ACTIVAR O INACTIVAR USUARIO
        public ActionResult CambiarEstado(int id)
        {
            Usuario usuario = db.Usuarios.Find(id);

            if (usuario == null)
            {
                return HttpNotFound();
            }

            if (usuario.Estado == "Activo")
            {
                usuario.Estado = "Inactivo";
            }
            else
            {
                usuario.Estado = "Activo";
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // HASH DE CONTRASEÑA
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
    }
}