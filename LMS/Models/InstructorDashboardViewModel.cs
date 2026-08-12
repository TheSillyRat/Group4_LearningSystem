namespace LMS.Models
{
    public class InstructorDashboardViewModel
    {
        public IEnumerable<Assignment> RecentAssignments { get; set; }

        public IEnumerable<Course> MyCourses { get; set; }
    }
}
