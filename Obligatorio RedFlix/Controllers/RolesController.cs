using Obligatorio_RedFlix.Filters;
using Obligatorio_RedFlix.Models;
using System.Linq;
using System.Web.Mvc;

namespace Obligatorio_RedFlix.Controllers
{
    [AutorizarRol("Administrador")]
    public class RolesController : Controller
    {
        private RedFlixDBEntities db = new RedFlixDBEntities();

        public ActionResult Index()
        {
            var roles = db.Roles.ToList();

            return View(roles);
        }

        // DETALLES
        public ActionResult Details(int id)
        {
            Role role = db.Roles.Find(id);

            if (role == null)
            {
                return HttpNotFound();
            }

            return View(role);
        }

        // CREAR - GET
        public ActionResult Create()
        {
            return View();
        }

        // CREAR - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Role role)
        {
            bool nombreExiste = db.Roles.Any(r => r.Nombre == role.Nombre);

            if (nombreExiste)
            {
                ModelState.AddModelError("Nombre", "Ya existe un rol con ese nombre.");
            }

            if (ModelState.IsValid)
            {
                db.Roles.Add(role);
                db.SaveChanges();

                TempData["Success"] = "Rol creado correctamente.";
                return RedirectToAction("Index");
            }

            return View(role);
        }

        // EDITAR - GET
        public ActionResult Edit(int id)
        {
            Role role = db.Roles.Find(id);

            if (role == null)
            {
                return HttpNotFound();
            }

            return View(role);
        }

        // EDITAR - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Role role)
        {
            Role roleBD = db.Roles.Find(role.IdRol);

            if (roleBD == null)
            {
                return HttpNotFound();
            }

            bool nombreExiste = db.Roles.Any(r => r.Nombre == role.Nombre && r.IdRol != role.IdRol);

            if (nombreExiste)
            {
                ModelState.AddModelError("Nombre", "Ya existe otro rol con ese nombre.");
            }

            if (ModelState.IsValid)
            {
                roleBD.Nombre = role.Nombre;
                db.SaveChanges();

                TempData["Success"] = "Rol actualizado correctamente.";
                return RedirectToAction("Index");
            }

            return View(role);
        }

        // ELIMINAR - GET (confirmación)
        public ActionResult Delete(int id)
        {
            Role role = db.Roles.Find(id);

            if (role == null)
            {
                return HttpNotFound();
            }

            return View(role);
        }

        // ELIMINAR - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Role role = db.Roles.Find(id);

            if (role == null)
            {
                return HttpNotFound();
            }

            bool tieneUsuarios = db.Usuarios.Any(u => u.IdRol == id);

            if (tieneUsuarios)
            {
                TempData["Error"] = "No se puede eliminar el rol porque hay usuarios asignados a él.";
                return RedirectToAction("Index");
            }

            db.Roles.Remove(role);
            db.SaveChanges();

            TempData["Success"] = "Rol eliminado correctamente.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}