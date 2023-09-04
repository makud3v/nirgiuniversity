using Microsoft.AspNetCore.Mvc;

namespace nirgi_mvc.Controllers
{
    public class InstructorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
