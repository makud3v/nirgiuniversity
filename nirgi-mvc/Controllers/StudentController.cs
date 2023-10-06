using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nirgi_mvc.Data;
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


        // GET: Student/
        public async Task<IActionResult> Index()
        {
            return View(await _context.Students.ToListAsync());
        }


        // GET: Student/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            var student = await GetStudentById(id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }


        // GET: Student/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            var student = await GetStudentById(id);
            if (student == null)
            {
                return NotFound();
            }

            ViewBag.Courses = await _context.Courses.ToListAsync();
            return View(student);
        }

        // POST: Student/Edit/{student}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Student std, int[] enrolledCourses, string[] courseGrades)
        {
            try
            {
                // get a student instance by id
                Student student = await GetStudentById(std.Id);

                // clear their enrollments and replace with new ones
                student.Enrollments.Clear();

                int i = 0;
                foreach(var courseID in enrolledCourses)
                {
                    var course = await _context.Courses.Where(c => c.CourseID == courseID).FirstOrDefaultAsync();
                    if (course == null) 
                        return BadRequest();


                    // create a new enrollment for them
                    var grade = courseGrades.ElementAtOrDefault(i);
                    var enrollment = new Enrollment()
                    {
                        Course = course,
                        CourseID = courseID,
                        Student = student,
                        StudentID = student.Id,
                        Grade = grade != null ? ParseStringToGrade(grade) : null
                    };
                    student.Enrollments.Add(enrollment);
                    i++;
                }

                // update basic details
                await TryUpdateModelAsync(student, "",
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
            return RedirectToAction(nameof(Index));
        }

        // GET: Student/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            return View(await GetStudentById(id));
        }


        // POST: Student/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            Student student = await GetStudentById(id);
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Student/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Student/Create/{student}
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


        private Grade? ParseStringToGrade(string grade)
        {
            switch(grade)
            {
                case "A":return Grade.A;
                case "B": return Grade.B;
                case "C":return Grade.C;
                case "D":return Grade.D;
                case "F": return Grade.F;
            }
            return null;
        }

        private async Task<Student> GetStudentById(int? id)
        {
            return await _context.Students
                .Include(s => s.Enrollments)
                .ThenInclude(equals => equals.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
