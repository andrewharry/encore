using Encore.Tutorials.TutorialConsole.Services.Support;
using Microsoft.Extensions.Logging;

namespace Encore.Tutorials.TutorialConsole.Services;

public class StudentUpdateService
{
    private readonly IStudentLookup lookup;
    private readonly IStudentRepository update;
    private readonly ILogger<StudentUpdateService> logger;

    public StudentUpdateService(IStudentLookup lookup, IStudentRepository update, ILogger<StudentUpdateService> logger)
    {
        this.lookup = lookup;
        this.update = update;
        this.logger = logger;
    }

    public async Task<bool> Update(int studentId, string name)
    {
        var existing = await lookup.GetStudent(studentId);

        if (existing == null)
        {
            logger.LogWarning($"Student not found with ID:{studentId}");
            return false;
        }

        existing.Name = name;

        return await update.Update(existing);
    }
}