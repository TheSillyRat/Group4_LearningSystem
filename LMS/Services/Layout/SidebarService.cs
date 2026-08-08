
using LMS.ViewModels.Layout;

namespace LMS.Services.Layout
{
    public class SidebarService
    {
        public SidebarViewModel GetSidebar(string role)
        {
            return role.ToLower() switch
            {
                "student" => GetStudentSidebar(),
                "instructor" => GetInstructorSidebar(),
                "admin" => GetAdminSidebar(),
                _ => new SidebarViewModel()
            };
        }

        private SidebarViewModel GetStudentSidebar()
        {
            return new SidebarViewModel
            {
                DisplayName = "Student Demo",
                Email = "student@gmail.com",
                RoleName = "Student",
                AvatarUrl = "/images/default-avatar.png",

                Items = new List<SidebarItemViewModel>
                {
                    Item("Dashboard", "bi-speedometer2", "/Student/Dashboard"),

                    Parent("Courses", "bi-book", new List<SidebarItemViewModel>
                    {
                        Item("Browse Courses", null, "/Student/Courses/Browse"),
                        Item("My Courses", null, "/Student/Courses"),
                        Item("Course Materials", null, "/Student/Courses/Materials")
                    }),

                    Parent("Assignments", "bi-file-earmark-text", new List<SidebarItemViewModel>
                    {
                        Item("My Assignments", null, "/Student/Assignments"),
                        Item("My Submissions", null, "/Student/Assignments/Submissions")
                    }),

                    Parent("Assessments", "bi-patch-question", new List<SidebarItemViewModel>
                    {
                        Item("Quizzes", null, "/Student/Quizzes"),
                        Item("Exams", null, "/Student/Exams")
                    }),

                    Item("Grades", "bi-bar-chart", "/Student/Grades"),

                    Item("Discussion", "bi-chat-dots", "/Student/Discussion"),

                    Item("Profile", "bi-person", "/Student/Profile")
                }
            };
        }

        private SidebarViewModel GetInstructorSidebar()
        {
            return new SidebarViewModel
            {
                DisplayName = "Instructor Demo",
                Email = "instructor@gmail.com",
                RoleName = "Instructor",
                AvatarUrl = "/images/default-avatar.png",

                Items = new List<SidebarItemViewModel>
                {
                    Item("Dashboard", "bi-speedometer2", "/Instructor/Dashboard"),

                    Parent("Courses", "bi-book", new List<SidebarItemViewModel>
                    {
                        Item("My Courses", null, "/Instructor/Courses"),
                        Item("Create Course", null, "/Instructor/Courses/Create"),
                        Item("Course Content", null, "/Instructor/Courses/Content")
                    }),

                    Parent("Students", "bi-people", new List<SidebarItemViewModel>
                    {
                        Item("Enrollments", null, "/Instructor/Students/Enrollments"),
                        Item("Attendance", null, "/Instructor/Students/Attendance"),
                        Item("Progress", null, "/Instructor/Students/Progress")
                    }),

                    Parent("Assignments", "bi-file-earmark-text", new List<SidebarItemViewModel>
                    {
                        Item("Manage Assignments", null, "/Instructor/Assignments"),
                        Item("Submissions", null, "/Instructor/Assignments/Submissions")
                    }),

                    Parent("Assessments", "bi-patch-question", new List<SidebarItemViewModel>
                    {
                        Item("Quizzes", null, "/Instructor/Assessments/Quizzes"),
                        Item("Exams", null, "/Instructor/Assessments/Exams"),
                        Item("Results / Grading", null, "/Instructor/Assessments/Results")
                    }),

                    Item("Discussion", "bi-chat-dots", "/Instructor/Discussion"),

                    Item("Profile", "bi-person", "/Instructor/Profile")
                }
            };
        }

        private SidebarViewModel GetAdminSidebar()
        {
            return new SidebarViewModel
            {
                DisplayName = "Admin Demo",
                Email = "admin@gmail.com",
                RoleName = "Admin",
                AvatarUrl = "/images/default-avatar.png",

                Items = new List<SidebarItemViewModel>
                {
                    Item("Dashboard", "bi-speedometer2", "/Admin/Dashboard"),

                    Item("Users", "bi-people", "/Admin/Users"),

                    Item("Courses", "bi-book", "/Admin/Courses"),

                    Item("Enrollments", "bi-person-check", "/Admin/Enrollments"),

                    Item("Profile", "bi-person", "/Admin/Profile")
                }
            };
        }

        private static SidebarItemViewModel Item(
            string title,
            string? icon,
            string? url)
        {
            return new SidebarItemViewModel
            {
                Title = title,
                Icon = icon,
                Url = url
            };
        }

        private static SidebarItemViewModel Parent(
            string title,
            string? icon,
            List<SidebarItemViewModel> children)
        {
            return new SidebarItemViewModel
            {
                Title = title,
                Icon = icon,
                Children = children
            };
        }
    }
}