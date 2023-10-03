namespace nirgi_mvc.Models.ViewModels
{
    public class CourseData
    {
        public Course Course { get; set; }
        public ICollection<Student> AllStudents { get; set; }
        public ICollection<Instructor> AllInstructors { get; set; }
        public ICollection<Student> AssignedStudents { get; set; }
        public ICollection<Instructor> AssignedInstructors { get; set; }
    }
}
