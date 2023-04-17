using Encore.Testing.Helpers;
using Encore.Tests.TestClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Encore.Tests
{
    [TestClass]
    public class TypeDependencyTests
    {
        [TestMethod]
        public void Should_opt_for_the_ideal_constructor()
        {
            var constructor = ConstructorLookup.Get(typeof(ConstructorTestClass));

            Assert.IsNotNull(constructor);

        }
    }
}