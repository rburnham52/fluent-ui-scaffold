using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Example tests demonstrating the new deferred execution chain pattern.
    /// These tests require the sample app to be running.
    /// </summary>
    [TestClass]
    public class HomePageTests
    {
        [TestMethod]
        [TestCategory("E2E")]
        public async Task HomePage_NavigateAndVerifyWelcome()
        {
            var session = await TestAssemblyHooks.App.CreateSessionAsync();
            try
            {
                await TestAssemblyHooks.App.NavigateTo<HomePage>()
                    .VerifyWelcomeVisible();
            }
            finally
            {
                await TestAssemblyHooks.App.DisposeSessionAsync();
            }
        }

        [TestMethod]
        [TestCategory("E2E")]
        public async Task HomePage_ClickCounter_Increments()
        {
            var session = await TestAssemblyHooks.App.CreateSessionAsync();
            try
            {
                await TestAssemblyHooks.App.NavigateTo<HomePage>()
                    .ClickCounter()
                    .ClickCounter()
                    .ClickCounter();
            }
            finally
            {
                await TestAssemblyHooks.App.DisposeSessionAsync();
            }
        }

        [TestMethod]
        [TestCategory("E2E")]
        public async Task HomePage_CrossPageNavigation_ToLogin()
        {
            var session = await TestAssemblyHooks.App.CreateSessionAsync();
            try
            {
                // Demonstrates page chaining: start on home, navigate to login
                await TestAssemblyHooks.App.NavigateTo<HomePage>()
                    .VerifyWelcomeVisible()
                    .NavigateTo<LoginPage>()
                    .ClickLoginTab()
                    .EnterEmail("test@example.com");
            }
            finally
            {
                await TestAssemblyHooks.App.DisposeSessionAsync();
            }
        }
    }
}
