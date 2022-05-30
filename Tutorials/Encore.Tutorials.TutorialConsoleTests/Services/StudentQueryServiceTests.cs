using Encore.EFCoreTesting;
using Encore.Tutorials.TutorialConsole.Data;
using Encore.Tutorials.TutorialConsole.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Encore.Tutorials.TutorialConsole.Services.Tests
{
    [TestClass]
    public class StudentQueryServiceTests : TestWithEFCore
    {
        [NotNull]
        private StudentQueryService Sut;

        protected override void OnPreRegistration()
        {
            base.OnPreRegistration();
            base.CreateDatabase<SchoolContext>();
        }

        protected override void OnSutRegistration()
        {
            Register<StudentQueryService>();
            base.OnSutRegistration();
        }

        protected override void OnSutResolve()
        {
            base.OnSutResolve();
            Sut = Resolve<StudentQueryService>();
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