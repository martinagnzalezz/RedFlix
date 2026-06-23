using Obligatorio_RedFlix.Models;
using System.Linq;
using System.Web.Mvc;

namespace Obligatorio_RedFlix.Controllers
{
    public class RolesController : Controller
    {
        private RedFlixDBEntities db = new RedFlixDBEntities();

        public ActionResult Index()
        {
            var roles = db.Roles.ToList();

            return View(roles);
        }
    }
}