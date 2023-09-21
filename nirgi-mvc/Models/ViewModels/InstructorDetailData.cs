using nirgi_mvc.Models;

namespace nirgi_mvc.Models.ViewModels
{
    public class InstructorDetailData
    {
        public Instructor Instructor { get; set; }
        public IEnumerable<Course> AllCourses { get; set; }
    }
}