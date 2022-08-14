using Encore.Testing;
using Encore.Tutorials.TutorialConsole.Data;
using Encore.Tutorials.TutorialConsole.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Encore.Tutorials.TutorialConsole.Services.Support;
using Microsoft.Extensions.DependencyInjection;

namespace Encore.Tutorials.TutorialConsole.Services.Tests
{
    [TestClass]
    public class StudentQueryServiceTests : TestWithDependencies<StudentQueryService>
    {
        protected override void OnPreRegistration()
        {
            CreateDatabase<SchoolContext>(ServiceLifetime.Transient);
        }

        [TestMethod]
        public async Task GetAllStudentsTest()
        {
            SetItems(new Student { Name = "John Smith" }, new Student { Name = "Jane Doe" });

            var students = await Sut.GetAllStudents();

            Assert.IsNotNull(students);
            Assert.AreEqual(2, students.Length);

            ExpectLogInfo("Calling GetAllStudents method");
        }
    }
}