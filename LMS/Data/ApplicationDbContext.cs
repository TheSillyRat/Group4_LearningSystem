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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Instructor)
                .WithMany(u => u.Assignments)
                .HasForeignKey(a => a.InstructorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ForumPost>()
                .HasOne(fp => fp.User)
                .WithMany(u => u.ForumPosts)
                .HasForeignKey(fp => fp.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ForumReply>()
                .HasOne(fr => fr.User)
                .WithMany(u => u.ForumReplies)
                .HasForeignKey(fr => fr.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
