using Microsoft.VisualStudio.TestTools.UnitTesting;
using Encore.Testing.Services;
using Encore.Tests.TestClasses;

namespace Encore.Tests
{
    [TestClass]
    public class TypeDependencyTests
    {
        [TestMethod]
        public void Should_opt_for_the_ideal_constructor()
        {
            var constructor = TypeDependencies.GetConstructor(typeof(ConstructorTestClass));

            Assert.IsNotNull(constructor);

        }
    }
}