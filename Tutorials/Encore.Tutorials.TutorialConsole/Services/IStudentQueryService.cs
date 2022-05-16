using Encore.Tutorials.TutorialConsole.Models;

namespace Encore.Tutorials.TutorialConsole.Services
{
    public interface IStudentQueryService
    {
        Task<Student[]> GetAllStudents();
    }
}