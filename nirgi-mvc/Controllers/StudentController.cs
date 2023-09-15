using Microsoft.AspNetCore.Mvc;
using nirgi_mvc.Data;
using Microsoft.EntityFrameworkCore;
using nirgi_mvc.Models;

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
            var student = await getStudent(id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            var student = await getStudent(id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Student std)
        {
            try
            {
                Student student = _context.Students.Where(student => student.Id == std.Id).FirstOrDefault();
                await TryUpdateModelAsync<Student>(student, "",
                    s => s.FirstName,
                    s => s.LastName,
                    s => s.EnrollmentDate
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return RedirectToAction(nameof(Index));
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Edit");
        }


        public async Task<IActionResult> Delete(int? id)
        {
            return View(await getStudent(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Student student)
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("EnrollmentDate,FirstName,LastName")] Student student)
        {
            try
            {
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes");
            }

            return View(student);
        }


        private async Task<Student> getStudent(int? id)
        {
            return await _context.Students
                .Include(s => s.Enrollments)
                .ThenInclude(equals => equals.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
