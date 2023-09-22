using Microsoft.AspNetCore.Mvc;
using nirgi_mvc.Data;
using nirgi_mvc.Models.ViewModels;
using nirgi_mvc.Models;
using Microsoft.EntityFrameworkCore;

namespace nirgi_mvc.Controllers
{
    public class InstructorController : Controller
    {
        private readonly MvcUniversityContext _context;

        public InstructorController(MvcUniversityContext context)
        {
            _context = context;
        }


        // GET: Instructor/
        public async Task<IActionResult> Index(int? id, int? courseID)
        {
            var viewModel = new InstructorIndexData();
            viewModel.Instructors = await _context.Instructors
                  .Include(i => i.OfficeAssignment)
                  .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                        .ThenInclude(i => i.Department)
                  .OrderBy(i => i.LastName)
                  .ToListAsync();

            if (id != null)
            {
                ViewData["InstructorID"] = id.Value;
                Instructor instructor = viewModel.Instructors.Where(
                    i => i.Id == id.Value).Single();
                viewModel.Courses = instructor.CourseAssignments.Select(s => s.Course);
            }

            if (courseID != null)
            {
                ViewData["CourseID"] = courseID.Value;
                var selectedCourse = viewModel.Courses.Where(x => x.CourseID == courseID).Single();
                await _context.Entry(selectedCourse).Collection(x => x.Enrollments).LoadAsync();
                foreach (Enrollment enrollment in selectedCourse.Enrollments)
                {
                    await _context.Entry(enrollment).Reference(x => x.Student).LoadAsync();
                }
                viewModel.Enrollments = selectedCourse.Enrollments;
            }

            return View(viewModel);
        }
        

        // GET: Instructor/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Instructor/Create/{instructor}
        public async Task<IActionResult> Create(Instructor instructor)
        {
            instructor.HireDate = DateTime.Now;
            _context.Instructors.Add(instructor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Instructor/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            Instructor instructor = await GetInstructorById(id);
            InstructorDetailData instructorData = await GetInstructorData(instructor);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructorData);
        }

        // POST: Instructor/Edit/{instructor}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Instructor instructor, int[] courses)
        {
            Instructor instructorToUpdate = await GetInstructorById(instructor.Id);
            instructorToUpdate.FirstName = instructor.FirstName;
            instructorToUpdate.LastName = instructor.LastName;

            instructorToUpdate.OfficeAssignment ??= new OfficeAssignment();
            instructorToUpdate.OfficeAssignment.Location = instructor.OfficeAssignment.Location == null ? "" : instructor.OfficeAssignment.Location;
            instructorToUpdate.HireDate = instructor.HireDate;
            UpdateInstructorCourses(instructorToUpdate, courses);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Instructor/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            Instructor instructor = await GetInstructorById(id);
            InstructorDetailData instructorData = await GetInstructorData(instructor);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructorData);
        }


        // GET: Instructor/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            return View(await GetInstructorById(id));
        }

        // POST: Instructor/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            Instructor instructor = await GetInstructorById(id);

            var departments = await _context.Departments
                .Where(d => d.InstructorID == id)
                .ToListAsync();
            departments.ForEach(d => d.InstructorID = null);

            _context.Instructors.Remove(instructor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private void UpdateInstructorCourses(Instructor instructorToUpdate, int[] selectedCourses)
        {
            // Create a HashSet to efficiently track selected course IDs.
            var selectedCourseIds = new HashSet<int>(selectedCourses);

            // Create a list to store course assignments to remove.
            var assignmentsToRemove = new List<CourseAssignment>();

            // Iterate through existing course assignments.
            foreach (var assignment in instructorToUpdate.CourseAssignments)
            {
                // If the assignment's course ID is not in the selected courses, mark it for removal.
                if (!selectedCourseIds.Contains(assignment.CourseID))
                {
                    assignmentsToRemove.Add(assignment);
                }
            }

            // Remove marked course assignments.
            foreach (var assignmentToRemove in assignmentsToRemove)
            {
                instructorToUpdate.CourseAssignments.Remove(assignmentToRemove);
            }

            // Add new course assignments for selected courses that don't have an assignment.
            foreach (var id in selectedCourses)
            {
                if (!instructorToUpdate.CourseAssignments.Any(assignment => assignment.CourseID == id))
                {
                    Course course = _context.Courses.FirstOrDefault(c => c.CourseID == id);
                    if (course != null)
                    {
                        instructorToUpdate.CourseAssignments.Add(new CourseAssignment
                        {
                            Instructor = instructorToUpdate,
                            InstructorID = instructorToUpdate.Id,
                            CourseID = id,
                            Course = course
                        });
                    }
                }
            }
        }


        private async Task<InstructorDetailData> GetInstructorData(Instructor instructor)
        {
            InstructorDetailData data = new()
            {
                Instructor = instructor,
                AllCourses = await _context.Courses
                .Include(i => i.Department)
                .ToListAsync()
            };

            return data;
        }

        private async Task<Instructor> GetInstructorById(int? id)
        {
            return await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(ca => ca.Course)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(ca => ca.Course.Enrollments)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
