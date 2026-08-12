using LMS.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            try
            {
                // Automatically apply Migrations if database does not exist
                context.Database.Migrate();
            }
            catch
            {
                // Bỏ qua cảnh báo PendingModelChangesWarning khi nạp dữ liệu mẫu
            }

            // Tự động bổ sung cột ImageUrl vào bảng Courses trong SQL Server nếu chưa có
            try
            {
                context.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Courses]') AND name = 'ImageUrl')
                    BEGIN
                        ALTER TABLE [Courses] ADD [ImageUrl] nvarchar(max) NULL;
                    END
                ");
            }
            catch { }

            // Tự động bổ sung cột ModuleId và CourseId vào bảng Quizzes trong SQL Server nếu chưa có
            try
            {
                context.Database.ExecuteSqlRaw(@"
                    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Quizzes')
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Quizzes]') AND name = 'ModuleId')
                        BEGIN
                            ALTER TABLE [Quizzes] ADD [ModuleId] int NULL;
                        END
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Quizzes]') AND name = 'CourseId')
                        BEGIN
                            ALTER TABLE [Quizzes] ADD [CourseId] int NULL;
                        END
                    END
                ");
            }
            catch { }

            // Tự động tạo bảng Questions nếu chưa tồn tại
            try
            {
                context.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Questions')
                    BEGIN
                        CREATE TABLE [Questions] (
                            [QuestionId] int NOT NULL IDENTITY,
                            [QuestionText] nvarchar(1000) NOT NULL,
                            [QuestionType] nvarchar(50) NOT NULL DEFAULT 'MultipleChoice',
                            [OptionA] nvarchar(300) NULL,
                            [OptionB] nvarchar(300) NULL,
                            [OptionC] nvarchar(300) NULL,
                            [OptionD] nvarchar(300) NULL,
                            [CorrectAnswer] nvarchar(500) NULL,
                            [QuizId] int NOT NULL,
                            CONSTRAINT [PK_Questions] PRIMARY KEY ([QuestionId]),
                            CONSTRAINT [FK_Questions_Quizzes_QuizId] FOREIGN KEY ([QuizId]) REFERENCES [Quizzes] ([QuizId]) ON DELETE CASCADE
                        );
                    END
                ");
            }
            catch { }

            // Tự động tạo bảng QuizResults nếu chưa tồn tại
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QuizResults')
                BEGIN
                    CREATE TABLE [QuizResults] (
                        [QuizResultId] int NOT NULL IDENTITY,
                        [QuizId] int NOT NULL,
                        [StudentId] int NOT NULL,
                        [Score] float NOT NULL,
                        [CorrectAnswers] int NOT NULL,
                        [TotalQuestions] int NOT NULL,
                        [SubmittedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_QuizResults] PRIMARY KEY ([QuizResultId]),
                        CONSTRAINT [FK_QuizResults_Quizzes_QuizId] FOREIGN KEY ([QuizId]) REFERENCES [Quizzes] ([QuizId]) ON DELETE CASCADE,
                        CONSTRAINT [FK_QuizResults_Users_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
                    );
                END
            ");

            // Tự động tạo bảng UserContentCompletion nếu chưa tồn tại
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserContentCompletion')
                BEGIN
                    CREATE TABLE [UserContentCompletion] (
                        [UserContentCompletionId] int NOT NULL IDENTITY,
                        [StudentId] int NOT NULL,
                        [ContentId] int NOT NULL,
                        [CompletedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_UserContentCompletion] PRIMARY KEY ([UserContentCompletionId]),
                        CONSTRAINT [FK_UserContentCompletion_Users_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE,
                        CONSTRAINT [FK_UserContentCompletion_Content_ContentId] FOREIGN KEY ([ContentId]) REFERENCES [Content] ([ContentId]) ON DELETE CASCADE
                    );
                END
            ");

            // 1. Add Roles
            if (!context.Role.Any())
            {
                var roles = new Role[]
                {
                    new Role { RoleName = "Admin" },
                    new Role { RoleName = "Instructor" },
                    new Role { RoleName = "Student" }
                };
                context.Role.AddRange(roles);
                context.SaveChanges();
            }

            var adminRole = context.Role.First(r => r.RoleName == "Admin");
            var instructorRole = context.Role.First(r => r.RoleName == "Instructor");
            var studentRole = context.Role.First(r => r.RoleName == "Student");

            // 2. Add Users (Instructors & Students)
            var seedUsers = new List<User>
            {
                // Admin
                new User { FullName = "System Administrator", Email = "admin@lms.com", Password = "123456", SecurityPassword = "123456", RoleId = adminRole.RoleId },
                
                // Instructors
                new User { FullName = "John Doe", Email = "instructor@lms.com", Password = "123456", SecurityPassword = "123456", RoleId = instructorRole.RoleId },
                new User { FullName = "Sarah Jenkins", Email = "sarah@lms.com", Password = "123456", SecurityPassword = "123456", RoleId = instructorRole.RoleId },
                new User { FullName = "Michael Chen", Email = "michael@lms.com", Password = "123456", SecurityPassword = "123456", RoleId = instructorRole.RoleId },
                new User { FullName = "Elena Rostova", Email = "elena@lms.com", Password = "123456", SecurityPassword = "123456", RoleId = instructorRole.RoleId },

                // Students
                new User { FullName = "Hoang Sang", Email = "hinhtun608@gmail.com", Password = "123456", SecurityPassword = "123456", RoleId = studentRole.RoleId },
                new User { FullName = "Jane Smith", Email = "student@lms.com", Password = "123456", SecurityPassword = "123456", RoleId = studentRole.RoleId },
                new User { FullName = "Alex Johnson", Email = "alex@lms.com", Password = "123456", SecurityPassword = "123456", RoleId = studentRole.RoleId },
                new User { FullName = "Emily Watson", Email = "emily@lms.com", Password = "123456", SecurityPassword = "123456", RoleId = studentRole.RoleId },
                new User { FullName = "David Miller", Email = "david@lms.com", Password = "123456", SecurityPassword = "123456", RoleId = studentRole.RoleId },
                new User { FullName = "Sophia Martinez", Email = "sophia@lms.com", Password = "123456", SecurityPassword = "123456", RoleId = studentRole.RoleId },
                new User { FullName = "Daniel Brown", Email = "daniel@lms.com", Password = "123456", SecurityPassword = "123456", RoleId = studentRole.RoleId },
                new User { FullName = "Olivia Wilson", Email = "olivia@lms.com", Password = "123456", SecurityPassword = "123456", RoleId = studentRole.RoleId },
                new User { FullName = "Liam Taylor", Email = "liam@lms.com", Password = "123456", SecurityPassword = "123456", RoleId = studentRole.RoleId }
            };

            foreach (var user in seedUsers)
            {
                if (!context.Users.Any(u => u.Email == user.Email))
                {
                    context.Users.Add(user);
                }
            }
            try { context.SaveChanges(); } catch { }

            // Fetch created instructor & student references
            var instructor1 = context.Users.First(u => u.Email == "instructor@lms.com");
            var instructor2 = context.Users.First(u => u.Email == "sarah@lms.com");
            var instructor3 = context.Users.First(u => u.Email == "michael@lms.com");
            var instructor4 = context.Users.First(u => u.Email == "elena@lms.com");

            var students = context.Users.Include(u => u.Role).Where(u => u.Role != null && u.Role.RoleName == "Student").ToList();

            // 3. Add Courses
            var seedCourses = new List<Course>
            {
                new Course
                {
                    CourseName = "C# & ASP.NET Core MVC Development",
                    Description = "Professional Web MVC application development with .NET Core and SQL Server.",
                    Prerequisite = "Basic C# Programming Knowledge",
                    Materials = "Lecture Slides, Hands-on Source Code",
                    InstructorId = instructor1.UserId
                },
                new Course
                {
                    CourseName = "Modern UI/UX Design with Figma & CSS",
                    Description = "Learn design systems, glassmorphism, responsive grids, and modern layout aesthetics.",
                    Prerequisite = "None",
                    Materials = "Figma Design Assets & CSS Templates",
                    InstructorId = instructor4.UserId
                },
                new Course
                {
                    CourseName = "Cloud Native Architecture & Microservices",
                    Description = "Build scalable, distributed systems using Docker, Kubernetes, and ASP.NET Core Microservices.",
                    Prerequisite = "C# & Docker Basics",
                    Materials = "Dockerfiles, Kubernetes Manifests, Repositories",
                    InstructorId = instructor3.UserId
                },
                new Course
                {
                    CourseName = "Database Engineering with SQL Server & EF Core",
                    Description = "Master relational modeling, indexing performance, migration strategies, and LINQ optimizations.",
                    Prerequisite = "SQL Fundamentals",
                    Materials = "Database Diagrams, SQL Scripts",
                    InstructorId = instructor2.UserId
                },
                new Course
                {
                    CourseName = "AI & Machine Learning Fundamentals in Python",
                    Description = "Introduction to neural networks, model training, computer vision, and Natural Language Processing.",
                    Prerequisite = "Basic Mathematics & Python",
                    Materials = "Jupyter Notebooks, Datasets",
                    InstructorId = instructor3.UserId
                },
                new Course
                {
                    CourseName = "Fullstack React 19 & Next.js Masterclass",
                    Description = "Build enterprise web applications with Server Components, SSR, and API integration.",
                    Prerequisite = "JavaScript & HTML/CSS",
                    Materials = "Next.js Starter Kits, GitHub Projects",
                    InstructorId = instructor1.UserId
                }
            };

            foreach (var course in seedCourses)
            {
                if (!context.Course.Any(c => c.CourseName == course.CourseName))
                {
                    context.Course.Add(course);
                }
            }
            try
            {
                context.SaveChanges();
            }
            catch
            {
                // Ignored if seed data already exists or constraints fail
            }

            // Fetch created courses
            var allCourses = context.Course.ToList();

            // 4. Add Modules & Assignments for each course if missing
            foreach (var crs in allCourses)
            {
                if (!context.Module.Any(m => m.CourseId == crs.CourseId))
                {
                    var mod1 = new Module
                    {
                        ModuleName = $"Module 1: Foundations of {crs.CourseName.Split('&')[0].Trim()}",
                        Description = "Core fundamentals, environment configuration, and initial project architecture.",
                        DisplayOrder = 1,
                        CourseId = crs.CourseId
                    };
                    var mod2 = new Module
                    {
                        ModuleName = $"Module 2: Advanced Topics & Hands-on Workshop",
                        Description = "Deep dive into real-world application building and best practices.",
                        DisplayOrder = 2,
                        CourseId = crs.CourseId
                    };
                    context.Module.AddRange(mod1, mod2);
                }

                if (!context.Assignments.Any(a => a.CourseId == crs.CourseId))
                {
                    var assign = new Assignment
                    {
                        AssignmentTitle = $"Final Project: {crs.CourseName.Split('&')[0].Trim()} Submission",
                        Description = "Implement the course capstone project and submit source code repository link.",
                        DueDate = DateTime.Now.AddDays(14),
                        CourseId = crs.CourseId,
                        InstructorId = crs.InstructorId
                    };
                    context.Assignments.Add(assign);
                }
            }
            try { context.SaveChanges(); } catch { }

            // 5. Add Enrollments across students and courses
            if (!context.Enrollment.Any() || context.Enrollment.Count() < 10)
            {
                var rand = new Random();
                foreach (var st in students)
                {
                    // Enroll each student into 2 to 3 random courses
                    var selectedCourses = allCourses.OrderBy(x => rand.Next()).Take(3).ToList();
                    foreach (var crs in selectedCourses)
                    {
                        if (!context.Enrollment.Any(e => e.StudentId == st.UserId && e.CourseId == crs.CourseId))
                        {
                            context.Enrollment.Add(new Enrollment
                            {
                                StudentId = st.UserId,
                                CourseId = crs.CourseId,
                                EnrollmentDate = DateTime.Now.AddDays(-rand.Next(1, 60)),
                                Progress = rand.Next(15, 95),
                                Attendance = true
                            });
                        }
                    }
                }
                try { context.SaveChanges(); } catch { }
            }
        }
    }
}
