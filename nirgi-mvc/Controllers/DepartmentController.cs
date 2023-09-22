using Microsoft.AspNetCore.Mvc;
using nirgi_mvc.Data;
using nirgi_mvc.Models;
using Microsoft.EntityFrameworkCore;


namespace nirgi_mvc.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly MvcUniversityContext _context;

        public DepartmentController(MvcUniversityContext context)
        {
            _context = context;
        }

        // GET: Department/
        public async Task<IActionResult> Index()
        {
            return View(await _context.Departments
                .Include(s => s.Administrator)
                .ToListAsync());
        }


        // GET: Department/Create
        public async Task<IActionResult> Create()
        {
            return View(await _context.Instructors.ToListAsync());
        }


        // POST: Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name", "Budget")] Department dept, int? administratorId)
        {
            if (dept == null)
            {
                return NotFound();
            }

            dept.Administrator = await _context.Instructors.Where(i => i.Id == administratorId).FirstOrDefaultAsync();
            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
