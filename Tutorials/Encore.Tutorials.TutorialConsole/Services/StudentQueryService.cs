using Encore.Tutorials.TutorialConsole.Data;
using Encore.Tutorials.TutorialConsole.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Encore.Tutorials.TutorialConsole.Services
{
    [Transient]
    public class StudentQueryService : IStudentQueryService
    {
        private readonly SchoolContext dbContext;
        private readonly ILogger<StudentQueryService> logger;

        public StudentQueryService(SchoolContext dbContext, ILogger<StudentQueryService> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public Task<Student[]> GetAllStudents()
        {
            logger.LogInformation($"Calling {nameof(GetAllStudents)} method");
            return dbContext.Students.ToArrayAsync();
        }
    }
}
