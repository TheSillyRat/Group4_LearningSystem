using LMS.Models;

namespace LMS.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any users.
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            var roles = new Role[]
            {
                new Role { RoleName = "Admin" },
                new Role { RoleName = "Instructor" },
                new Role { RoleName = "Student" }
            };
            foreach (Role r in roles)
            {
                context.Role.Add(r);
            }
            context.SaveChanges();

            var users = new User[]
            {
                new User { FullName = "Nguyễn Văn Sang", Email = "vsang@gmail.com", Password = "123", RoleId = 3 },
                new User { FullName = "Trần Hữu Sang", Email = "hsang@gmail.com", Password = "123", RoleId = 2 },
                new User { FullName = "Lê Tuấn", Email = "tuan@gmail.com", Password = "123", RoleId = 1 }
            };
            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            context.SaveChanges();

            var courses = new Course[]
            {
                new Course { CourseName = "C# for Beginners", Description = "Learn the basics of C# programming.", Prerequisite = "None", InstructorId = 2 },
                new Course { CourseName = "ASP.NET Core Web API", Description = "Build RESTful APIs with ASP.NET Core.", Prerequisite = "C# Basics", InstructorId = 2 },
                new Course { CourseName = "Entity Framework Core", Description = "Learn how to interact with databases using EF Core.", Prerequisite = "C# and SQL Basics", InstructorId = 2 }
            };
            foreach (Course c in courses)
            {
                context.Course.Add(c);
            }
            context.SaveChanges();

            var modules = new Module[]
            {
                new Module { ModuleName = "Introduction to C#", CourseId = 1 },
                new Module { ModuleName = "Variables and Data Types", CourseId = 1 },
                new Module { ModuleName = "Control Flow", CourseId = 1 }
            };
            foreach (Module m in modules)
            {
                context.Module.Add(m);
            }
            context.SaveChanges();
        }
    }
}
