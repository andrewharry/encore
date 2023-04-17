using Encore.Tutorials.TutorialConsole.Models;
using Microsoft.EntityFrameworkCore;

namespace Encore.Tutorials.TutorialConsole.Data
{
    public class SchoolContext : DbContext
    {
        public SchoolContext() : base() { }

        public SchoolContext(DbContextOptions options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
       
    }
}