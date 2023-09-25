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


        // GET: Department/Details
        public async Task<IActionResult> Details(int? id)
        {
            var department = await GetDepartmentById(id);
            if (id == null || department == null)
                return NotFound();


            return View(department);
        }


        // GET: Department/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            var department = await GetDepartmentById(id);
            if (id == null || department == null)
                return NotFound();


            ViewBag.Instructors = await _context.Instructors.ToListAsync();    
            return View(department);
        }

        // POST: Department/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Department department, int? adminId)
        {

            var departmentToUpdate = await GetDepartmentById(department.DepartmentID);
            if (departmentToUpdate == null)
                return NotFound();

            TryUpdateModelAsync(departmentToUpdate, "",
                s => s.Name,
                s => s.Budget
            );

            if (adminId != null)
            {
                departmentToUpdate.Administrator = await _context.Instructors.Where(instr => instr.Id == adminId)
                    .FirstOrDefaultAsync();
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Department/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            var department = await GetDepartmentById(id);
            if (id == null || department == null)
                return NotFound();

            return View(department);
        }

        // POST: Department/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int departmentId)
        {
            if (departmentId == null)
                return NotFound();
            var department = await _context.Departments.Where(dep => dep.DepartmentID == departmentId)
                .Include(dep => dep.Courses)
                .FirstOrDefaultAsync();

            try
            {
                _context.Departments.Remove(department);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to delete department");
                return RedirectToAction(nameof(Index));
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private async Task<Department>? GetDepartmentById(int? id)
        {
            if (id == null)
                return null;
            return await _context.Departments.Where(dep => dep.DepartmentID == id)
                .Include(dep => dep.Courses)
                .Include(dep => dep.Administrator)
                .FirstOrDefaultAsync();
        }
    }
}
