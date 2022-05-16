using Encore.Tutorials.TutorialConsole.Services;
using Microsoft.Extensions.Hosting;

public class Worker : IHostedService
{
    private readonly IStudentQueryService studentQueries;

    public Worker(IStudentQueryService studentQueries)
    {
        this.studentQueries = studentQueries;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var students = await studentQueries.GetAllStudents();

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