using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;

namespace SampleApp.Tests.TestBases
{
    public abstract class TodoPageTestsBase
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
        public async Task TodoPage_AddTodo_AppearsInList()
        {
            await SharedTestApp.App.NavigateTo<HomePage>()
                .NavigateTo<TodosPage>()
                .ClickTodosTab()
                .AddTodo("Automate regression suite", "high")
                .VerifyTodoVisible("Automate regression suite");
        }

        [TestMethod]
        [TestCategory("E2E")]
        public async Task TodoPage_CompleteTodo_CanClearCompleted()
        {
            await SharedTestApp.App.NavigateTo<HomePage>()
                .NavigateTo<TodosPage>()
                .ClickTodosTab()
                .AddTodo("Temporary task")
                .CompleteTodo(0)
                .ClearCompleted();
        }
    }
}
