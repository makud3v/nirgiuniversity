using Microsoft.AspNetCore.Mvc;

namespace nirgi_mvc.Controllers
{
    public class DepartmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
