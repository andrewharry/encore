using Encore.Tutorials.TutorialConsole.Services;
using Microsoft.Extensions.Hosting;

public class Worker : IHostedService
{
    private readonly IStudentLookup studentLookup;

    public Worker(IStudentLookup studentLookup)
    {
        this.studentLookup = studentLookup;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var students = await studentLookup.GetAllStudents();

        if (students == null)
        {
            throw new ArgumentNullException(nameof(students));
        }

        // Do something with students
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}