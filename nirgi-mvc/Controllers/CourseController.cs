using Microsoft.AspNetCore.Mvc;

namespace nirgi_mvc.Controllers
{
    public class CourseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
