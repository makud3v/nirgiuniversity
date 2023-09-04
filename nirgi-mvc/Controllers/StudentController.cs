using Microsoft.AspNetCore.Mvc;
using nirgi_mvc.Data;
using System.Data.Entity;

namespace nirgi_mvc.Controllers
{
    public class StudentController : Controller
    {
        private readonly MvcUniversityContext _context;

        public StudentController(MvcUniversityContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Students.ToListAsync());
        }
    }
}
