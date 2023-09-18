using Microsoft.AspNetCore.Mvc;

namespace nirgi_mvc.Controllers
{
    public class DepartmentController : Controller
    {
        // GET: Department/
        public IActionResult Index()
        {
            return View();
        }
    }
}
