using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Encore.Testing
{
    public abstract class TestWithHooks : TestFoundation, IDisposable
    {
        private bool initialised;
        private bool finalised;

        protected TestWithHooks()
        {
            // NOTE: ensuring Initialise is called at least once
            Initialise();
        }

        [TestInitialize]
        public void Initialise()
        {
            if (initialised)
                return;

            initialised = true;
            SetupRegistry();

            OnSetup();
            OnPreRegistration();
            OnSutRegistration();

            SetupResolver();
            OnPostResolver();

            OnSutResolve();
            OnPostResolve();
        }

        /// <summary>
        /// Step 01 - Hook to allow Test classes to run any Setup code prior to Registration process starting
        /// </summary>
        /// <remarks>
        /// Useful for setting up Mock Databases, Logging etc
        /// </remarks>
        protected virtual void OnSetup() { }

        /// <summary>
        /// Step 02 - Pre Registration Hook
        /// </summary>
        /// <remarks>
        /// Register any classes where you want to substitute or force a particular service etc.
        /// </remarks>
        protected virtual void OnPreRegistration() { }

        /// <summary>
        /// Step 03 - Hook to allow Test classes to Register SUT Service
        /// </summary>
        protected virtual void OnSutRegistration() { }

        /// <summary>
        /// Step 04 - The DI 'Service Container' has now been created.
        /// </summary>
        /// <remarks>
        /// You are able to resolve services from the Resolver
        /// </remarks>
        protected virtual void OnPostResolver() { }

        /// <summary>
        /// Step 05 - Hook to allow the Test classes to Resolve SUT service
        /// </summary>
        protected virtual void OnSutResolve() { }

        /// <summary>
        /// Step 06 - Executes custom test initialisation after the standard initialisation.
        /// </summary>
        /// <remarks>
        /// Resolve the system-under-test instance here from the container (e.g. "sut = ServiceProvider.GetRequiredService<ISutType>()")
        /// </remarks>
        protected virtual void OnPostResolve() { }

        [TestCleanup]
        public void Cleanup()
        {
            if (finalised)
                return;

            finalised = true;

            OnPreCleanup();
            OnPostCleanup();
            base.Dispose();
        }

        /// <summary>
        /// Executes custom test cleanup before the standard cleanup.
        /// </summary>
        protected virtual void OnPreCleanup() { }

        /// <summary>
        /// Executes custom test cleanup after the standard cleanup.
        /// </summary>
        protected virtual void OnPostCleanup() { }

        public new void Dispose()
        {
            Cleanup();
            base.Dispose();
        }
    }
}
