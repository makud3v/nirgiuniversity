using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using nirgi_mvc.Data;
using nirgi_mvc.Models;

namespace nirgi_mvc.Controllers
{
    public class CourseController : Controller
    {
        private readonly MvcUniversityContext _context;

        public CourseController(MvcUniversityContext context)
        {
            _context = context;
        }


        // GET: Course/
        public async Task<IActionResult> Index()
        {
            return View(await _context.Courses
                .Include(c => c.Department)
                .ToListAsync());
        }

        // GET: Course/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            var course = await getCourse(id);
            if (course == null)
                return NotFound();

            return View(course);
        }

        // GET: Course/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.DepartmentId = new SelectList(_context.Departments.AsNoTracking(), "DepartmentID", "Name");

            return View();
        }

        // POST: Course/Create/{course}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course)
        {
            try
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes");
            }

            return View(course);
        }

        // GET: Course/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            var course = await getCourse(id);
            if (course == null)
                return NotFound();

            ViewBag.DepartmentId = new SelectList(_context.Departments.AsNoTracking(), "DepartmentID", "Name");

            return View(course);
        }

        // POST: Course/Edit/{course}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Course cs)
        {
            try
            {
                Course course = _context.Courses.Where(course => cs.CourseID == course.CourseID).FirstOrDefault();
                await TryUpdateModelAsync<Course>(course, "",
                    c => c.Title,
                    c => c.Credits,
                    c => c.DepartmentID
                );
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Course/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            return View(await getCourse(id));
        }

        // POST: Course/Delete/{course}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Course course)
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<Course> getCourse(int? id)
        {
            return await _context.Courses
                .Include(c => c.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CourseID == id);
        }
    }
}
