using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;

namespace SampleApp.Tests.TestBases
{
    public abstract class HomePageTestsBase
    {
        [TestInitialize]
        public virtual async Task Setup()
        {
            await SharedTestApp.App.CreateSessionAsync();
        }

        [TestCleanup]
        public virtual async Task Cleanup()
        {
            await SharedTestApp.App.DisposeSessionAsync();
        }

        [TestMethod]
        [TestCategory("E2E")]
        public async Task HomePage_DisplaysWelcomeMessage()
        {
            await SharedTestApp.App.NavigateTo<HomePage>()
                .VerifyWelcomeVisible();
        }

        [TestMethod]
        [TestCategory("E2E")]
        public async Task HomePage_DisplaysWeatherData()
        {
            await SharedTestApp.App.NavigateTo<HomePage>()
                .VerifyWeatherDataVisible();
        }

        [TestMethod]
        [TestCategory("E2E")]
        public async Task HomePage_ClickCounter_Increments()
        {
            await SharedTestApp.App.NavigateTo<HomePage>()
                .ClickCounter()
                .ClickCounter()
                .ClickCounter();
        }

        [TestMethod]
        [TestCategory("E2E")]
        public async Task HomePage_VerifyTitle()
        {
            await SharedTestApp.App.NavigateTo<HomePage>()
                .VerifyTitle("FluentUIScaffold");
        }
    }
}
