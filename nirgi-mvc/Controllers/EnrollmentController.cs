using Microsoft.AspNetCore.Mvc;
using nirgi_mvc.Data;
using Microsoft.EntityFrameworkCore;
using nirgi_mvc.Models;

namespace nirgi_mvc.Controllers
{
    public class EnrollmentController : Controller
    {
        private readonly MvcUniversityContext _context;

        public EnrollmentController(MvcUniversityContext context)
        {
            _context = context;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Enrollment e)
        {
            Console.WriteLine(e.ToString());
            try
            {
                var enrollment = _context.Enrollments.Where(enrollment => enrollment.EnrollmentID == e.EnrollmentID).FirstOrDefault();
                await TryUpdateModelAsync<Enrollment>(enrollment, "",
                    s => s.Grade
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Edit");
        }
    }
}
