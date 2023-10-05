using Microsoft.AspNetCore.Mvc;
using nirgi_mvc.Data;
using nirgi_mvc.Models;
using System.Diagnostics;

namespace nirgi_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly MvcUniversityContext _context;
        public HomeController(MvcUniversityContext context)
        {
            _context = context;
        }

        // GET: Home/
        public IActionResult Index()
        {
            ViewBag.StudentCount = _context.Students.Count();
            ViewBag.CourseCount = _context.Courses.Count();
            ViewBag.InstructorCount = _context.Instructors.Count();
            return View();
        }

        public IActionResult Privacy() { return View(); }

        // GET: Home/Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}