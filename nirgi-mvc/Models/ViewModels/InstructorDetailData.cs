using nirgi_mvc.Models;

namespace nirgi_mvc.Models.ViewModels
{
    public class InstructorDetailData
    {
        public IEnumerable<Instructor> Instructor { get; set; }
        public IEnumerable<CourseAssignment> AssignedCourses { get; set; }
        public IEnumerable<Course> AllCourses { get; set; }
    }
}