using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;

namespace SampleApp.Tests.TestBases
{
    public abstract class NavigationTestsBase
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
        public async Task Navigation_HomeToLogin_FluentChain()
        {
            await SharedTestApp.App.NavigateTo<HomePage>()
                .VerifyWelcomeVisible()
                .NavigateTo<LoginPage>()
                .ClickLoginTab()
                .EnterEmail("test@example.com");
        }

        [TestMethod]
        [TestCategory("E2E")]
        public async Task Navigation_HomeToProfile_EditAndCancel()
        {
            await SharedTestApp.App.NavigateTo<HomePage>()
                .NavigateTo<ProfilePage>()
                .ClickProfileTab()
                .ClickEdit()
                .EnterFirstName("Test")
                .EnterLastName("User")
                .Cancel();
        }

        [TestMethod]
        [TestCategory("E2E")]
        public async Task Navigation_HomeToTodos_AddAndVerify()
        {
            await SharedTestApp.App.NavigateTo<HomePage>()
                .NavigateTo<TodosPage>()
                .ClickTodosTab()
                .AddTodo("Navigation test todo")
                .VerifyTodoVisible("Navigation test todo");
        }

        [TestMethod]
        [TestCategory("E2E")]
        public async Task On_ResolvesCurrentPage_WithoutNavigation()
        {
            await SharedTestApp.App.NavigateTo<HomePage>()
                .VerifyWelcomeVisible();

            await SharedTestApp.App.On<HomePage>()
                .ClickCounter();
        }
    }
}
