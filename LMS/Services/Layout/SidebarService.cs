
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
                DisplayName = "",
                Email = "",
                RoleName = "Student",
                AvatarUrl = null,

                Items = new List<SidebarItemViewModel>
                {
                    Item("Dashboard", "bi-speedometer2", "/Student/Dashboard"),

                    Parent("Courses", "bi-book", new List<SidebarItemViewModel>
                    {
                        Item("Browse Courses", null, "/Student/Course/Browse"),
                        Item("My Courses", null, "/Student/Course"),
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
                DisplayName = "",
                Email = "",
                RoleName = "Instructor",
                AvatarUrl = null,

                Items = new List<SidebarItemViewModel>
{
                Item("Dashboard", "bi-speedometer2", "/Instructor/Dashboard"),

                Parent("Courses", "bi-book", new List<SidebarItemViewModel>
                {
                    Item("My Courses", null, "/Instructor/Course"),
                    Item("Create Course", null, "/Instructor/Course/Create"),
                    Item("Course Content", null, "/Instructor/Content")
                }),

                Parent("Students", "bi-people", new List<SidebarItemViewModel>
                {
                    Item("Enrollments", null, "#"),
                    Item("Attendance", null, "#"),
                    Item("Progress", null, "#")
                }),

                Parent("Assignments", "bi-file-earmark-text", new List<SidebarItemViewModel>
                {
                    Item("Manage Assignments", null, "/Instructor/Assignment"),
                    Item("Create Assignment", null, "/Instructor/Assignment/Create"),
                    Item("Submissions", null, "/Instructor/Assignment/Submissions")
                }),

                Parent("Assessments", "bi-patch-question", new List<SidebarItemViewModel>
                {
                    Item("Quizzes", null, "/Instructor/Quiz"),
                    Item("Exams", null, "/Instructor/Assessment"),
                    Item("Results / Grading", null, "#")
                }),

                Item("Discussion", "bi-chat-dots", "#"),

                Item("Profile", "bi-person", "#")
            }
            };
        }

        private SidebarViewModel GetAdminSidebar()
        {
            return new SidebarViewModel
            {
                DisplayName = "",
                Email = "",
                RoleName = "Admin",
                AvatarUrl = null,

                Items = new List<SidebarItemViewModel>
                {
                    Item("Dashboard", "bi-speedometer2", "/Admin/Dashboard"),

                    Item("Users", "bi-people", "/Admin/User"),

                    Item("Courses", "bi-book", "/Admin/Course"),

                    Item("Enrollments", "bi-person-check", "/Admin/Enrollment"),

                    Item("Assignments", "bi-file-earmark-text", "/Admin/Assignment")
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