using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using nirgi_mvc.Data;
using nirgi_mvc.Models;
using nirgi_mvc.Models.ViewModels;

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
            var course = await GetCourseById(id);
            if (course == null)
                return NotFound();

            var allStudents = await _context.Students
                .Include(s => s.Enrollments)
                .AsNoTracking()
                .ToListAsync();

            var allInstructors = await _context.Instructors
                .Include(s => s.CourseAssignments)
                .AsNoTracking()
                .ToListAsync();

            CourseData courseData = new()
            {
                Course = course,
                AllStudents = allStudents,
                AllInstructors = allInstructors,
                AssignedStudents = allStudents.Where(student => student.Enrollments.Where(enrollment => enrollment.CourseID == course.CourseID).Count() != 0).ToList(),
                AssignedInstructors = allInstructors.Where(instructor => instructor.CourseAssignments.Where(assignment => assignment.CourseID == course.CourseID).Count() != 0).ToList()
            };

            return View(courseData);
        }

        // GET: Course/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.DepartmentId = new SelectList(_context.Departments.AsNoTracking(), "DepartmentID", "Name");
            CourseData courseData = new()
            {
                Course = new(),
                AllStudents = _context.Students.AsNoTracking().ToList(),
                AllInstructors = _context.Instructors.AsNoTracking().ToList(),
                AssignedStudents = new List<Student>(),
                AssignedInstructors = new List<Instructor>()
            };


            return View(courseData);
        }

        // POST: Course/Create/{courseData}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseData courseData, int[]? assignedInstructors, int[]? assignedStudents)
        {
            try
            {
                await UpdateCourseAssignees(courseData.Course, assignedInstructors, assignedStudents);
                _context.Courses.Add(courseData.Course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unable to save changes");
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Course/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            var course = await GetCourseById(id);
            if (course == null)
                return NotFound();

            var allStudents = await _context.Students
                .Include(s => s.Enrollments)
                .AsNoTracking()
                .ToListAsync();

            var allInstructors = await _context.Instructors
                .Include(s => s.CourseAssignments)
                .AsNoTracking()
                .ToListAsync();

            CourseData courseData = new()
            {
                Course = course,
                AllStudents = allStudents,
                AllInstructors = allInstructors,
                AssignedStudents = allStudents.Where(student => student.Enrollments.Where(enrollment => enrollment.CourseID == course.CourseID).Count() != 0).ToList(),
                AssignedInstructors = allInstructors.Where(instructor => instructor.CourseAssignments.Where(assignment => assignment.CourseID == course.CourseID).Count() != 0).ToList()
            };

            ViewBag.DepartmentId = new SelectList(_context.Departments.AsNoTracking(), "DepartmentID", "Name");
            return View(courseData);
        }

        // POST: Course/Edit/{course}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [Bind("CourseID, Title, Credits, DepartmentID")] Course course, 
            int[]? assignedInstructors, 
            int[]? assignedStudents)
        {
            var courseInstance = await _context.Courses
                .Where(c => c.CourseID == course.CourseID)
                .Include(c => c.Department)
                .Include(c => c.CourseAssignments)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync();
            await UpdateCourseAssignees(courseInstance, assignedInstructors, assignedStudents);

            if (courseInstance.Department == null)
                courseInstance.Department = await _context.Departments.Where(d => d.DepartmentID == course.DepartmentID).FirstOrDefaultAsync();
            courseInstance.Title = course.Title;
            courseInstance.Credits = course.Credits;
            courseInstance.DepartmentID = course.DepartmentID;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Course/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            return View(await GetCourseById(id));
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

        private async Task<Course> GetCourseById(int? id)
        {
            var result = await _context.Courses
                .Include(c => c.Department)
                .Include(c => c.Enrollments)
                .Include(c => c.CourseAssignments)
                    .ThenInclude(ca => ca.Instructor)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CourseID == id);

            return result;
        }

        private async Task<bool> UpdateCourseAssignees(Course course, int[]? assignedInstructors, int[]? assignedStudents)
        {
            if (assignedInstructors == null && assignedStudents == null) return false;

            if (course.CourseAssignments == null)
                course.CourseAssignments = new List<CourseAssignment>();
            course.CourseAssignments.Clear();

            if (course.Enrollments == null)
                course.Enrollments = new List<Enrollment>();
            course.Enrollments.Clear();

            foreach (int id in assignedInstructors)
            {
                var instructor = await _context.Instructors.Where(instructor => instructor.Id == id).FirstOrDefaultAsync();
                var courseAssignment = new CourseAssignment()
                {
                    Course = course,
                    CourseID = course.CourseID,
                    Instructor = instructor,
                    InstructorID = id
                };
                course.CourseAssignments.Add(courseAssignment);

                if (instructor.CourseAssignments == null)
                    instructor.CourseAssignments = new List<CourseAssignment>();
                instructor.CourseAssignments.Add(courseAssignment);
            }

            foreach (int id in assignedStudents)
            {
                var student = await _context.Students.Where(student => student.Id == id).FirstOrDefaultAsync();
                var enrollment = new Enrollment()
                {
                    Student = student,
                    StudentID = id,
                    Course = course,
                    CourseID = course.CourseID
                };
                if (student.Enrollments == null)
                    student.Enrollments = new List<Enrollment>();
                student.Enrollments.Add(enrollment);
            }

            return true;
        }
    }
}
