using Encore.Tutorials.TutorialConsole.Models;

namespace Encore.Tutorials.TutorialConsole.Services
{
    public interface IStudentLookup
    {
        Task<Student[]> GetAllStudents();
        Task<Student?> GetStudent(int id);
    }
}