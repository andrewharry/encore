using System.Linq;
using Encore.Testing;
using Encore.Tutorials.TutorialConsole.Data;
using Encore.Tutorials.TutorialConsole.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Encore.Tutorials.TutorialConsole.Services.Support;
using NSubstitute;

namespace Encore.Tutorials.TutorialConsole.Services.Tests
{
    [TestClass]
    public class StudentUpdateServiceTests : TestWithDependencies<StudentUpdateService>
    {
        private IStudentRepository? studentRepository;

        protected override void OnPreRegistration()
        {
            CreateDatabase<SchoolContext>(ServiceLifetime.Transient);
            studentRepository = RegisterMock<IStudentRepository>();
        }

        [TestMethod]
        public async Task UpdateStudent()
        {
            var newName = "Jane Doe";
            var existing = SetItem(new Student { Name = "John Smith" });

            SetUpdateResult(shouldUpdate: true);

            var result = await Sut.Update(existing.Value.StudentId, newName);

            Assert.IsTrue(result);

            Assert.AreNotEqual(newName, existing.Value.Name);

            ExpectLogInfo($"Calling GetStudent method with Id:{existing.Value.StudentId}");
        }

        private void SetUpdateResult(bool shouldUpdate)
        {
            studentRepository?
                .Update(Arg.Any<Student>())
                .ReturnsForAnyArgs(shouldUpdate);
        }
    }
}