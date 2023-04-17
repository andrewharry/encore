using Encore.Tutorials.TutorialConsole.Data;
using Encore.Tutorials.TutorialConsole.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Encore.Tutorials.TutorialConsole.Services
{
    [Transient]
    public class StudentLookup : IStudentLookup
    {
        private readonly SchoolContext dbContext;
        private readonly ILogger<StudentLookup> logger;

        public StudentLookup(SchoolContext dbContext, ILogger<StudentLookup> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task<Student[]> GetAllStudents()
        {
            logger.LogInformation($"Calling {nameof(GetAllStudents)} method");
            return (await dbContext.Students.ToArrayAsync()).ToSafeArray();
        }

        public Task<Student?> GetStudent(int id)
        {
            logger.LogInformation($"Calling {nameof(GetStudent)} method with Id:{id}");
            return dbContext.Students.FirstOrDefaultAsync(v => v.StudentId == id);
        }
    }
}