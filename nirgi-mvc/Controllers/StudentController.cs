using Microsoft.AspNetCore.Mvc;
using nirgi_mvc.Data;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Enrollments)
                .ThenInclude(equals => equals.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }
    }
}
