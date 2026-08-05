using Microsoft.EntityFrameworkCore;
using LMS.Models;


namespace LMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }

        public DbSet<Course> Course { get; set; }

        public DbSet<Enrollment> Enrollment { get; set; }

        public DbSet<Module> Module { get; set; }

        public DbSet<Content> Content { get; set; }

        public DbSet<Assignment> Assignments { get; set; }

        public DbSet<Submission> Submissions { get; set; }

        public DbSet<ForumPost> ForumPosts { get; set; }
        public DbSet<Role> Role { get; set; }

        public DbSet<Quiz> Quizzes { get; set; }


        public DbSet<ForumReply> ForumReplies { get; set; }
    }
}
