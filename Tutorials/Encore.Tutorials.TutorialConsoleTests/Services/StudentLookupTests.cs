﻿using System.Threading.Tasks;
using Encore.Testing;
using Encore.Tutorials.TutorialConsole.Data;
using Encore.Tutorials.TutorialConsole.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Encore.Tutorials.TutorialConsole.Services.Tests
{
    [TestClass]
    public class StudentLookupTests : TestWithDependencies<StudentLookup>
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