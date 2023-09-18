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

        // POST: Instructor/Create/{instructor}
        public async Task<IActionResult> Create(Instructor instructor)
        {
            _context.Instructors.Add(instructor);
            // finish later pls
            return RedirectToAction(nameof(Index));
        }


        // GET: Instructor/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            Instructor instructor = await getInstructorById(id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // POST: Instructor/Edit/{instructor}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Instructor instr, string[] selectedCourses)
        {
            if (instr == null)
            {
                return NotFound();
            }

            var instructorToUpdate = await _context.Instructors.Where(instructor => instructor.Id == instr.Id)
                .FirstOrDefaultAsync();

            if (await TryUpdateModelAsync<Instructor>(
                instructorToUpdate,
                "",
                i => i.FirstName, 
                i => i.LastName, 
                i => i.HireDate, 
                i => i.OfficeAssignment
                ))
            {
                if (String.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment?.Location))
                {
                    instructorToUpdate.OfficeAssignment = null;
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Unable to save changes.");
                    throw ex;
                }


                return RedirectToAction(nameof(Index));
            }


            return View(instructorToUpdate);
        }


        // GET: Instructor/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            return View(await getInstructorById(id));
        }


        // GET: Instructor/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            return View(await getInstructorById(id));
        }

        // POST: Instructor/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            Instructor instructor = await getInstructorById(id);

            var departments = await _context.Departments
                .Where(d => d.InstructorID == id)
                .ToListAsync();
            departments.ForEach(d => d.InstructorID = null);

            _context.Instructors.Remove(instructor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool populateAssignedCourses(InstructorDetailData viewData)
        {
            if (viewData == null)
            {
                return false;
            }

            foreach (var courseAssignment in _context.CourseAssignments)
            {
                if (courseAssignment.Instructor == viewData.Instructor)
                {
                    viewData.AssignedCourses.Append(courseAssignment);
                }
                else
                {
                    if (viewData.AssignedCourses.Contains(courseAssignment))
                    {
                        viewData.AssignedCourses.ToList<CourseAssignment>().Remove(courseAssignment);
                    }
                }
            }

            return true;
        }

        private void updateCourseData(Instructor instructor, string[] activeCourses)
        {
            



 
        }

        private async Task<Instructor> getInstructorById(int? id)
        {
            return await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments).ThenInclude(i => i.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
