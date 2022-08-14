using Encore.Tutorials.TutorialConsole.Models;

namespace Encore.Tutorials.TutorialConsole.Services.Support;

public interface IStudentRepository
{
    public Task<bool> Update(Student student);
}