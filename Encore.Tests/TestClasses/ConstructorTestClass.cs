using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;

namespace Encore.Tests.TestClasses
{
    public class ConstructorTestClass
    {
        public ILogger<ConstructorTestClass> Logger { get; }

        private readonly IEnumerable<string>? ignoredParams;

        [DoNotSelect]
        public ConstructorTestClass(IEnumerable<string> ignoredParams) : this()
        {
            this.ignoredParams = ignoredParams;
        }

        /// <summary>
        /// This constructor is the target for the TypeDependencies.GetConstructor() 
        /// </summary>
        public ConstructorTestClass(ILogger<ConstructorTestClass> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// This constructor should not be selected as there is a contructor with more dependencies above
        /// </summary>
        public ConstructorTestClass()
        {
            Logger = new NullLogger<ConstructorTestClass>();
        }
    }
}
