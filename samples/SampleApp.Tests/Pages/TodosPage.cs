using System;

using FluentUIScaffold.Core.Pages;

using Microsoft.Playwright;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Page object for the Todos tab of the sample app.
    /// Demonstrates form interaction and dynamic content via deferred execution.
    /// </summary>
    public class TodosPage : Page<TodosPage>
    {
        protected TodosPage(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        public TodosPage ClickTodosTab()
        {
            return Enqueue<IPage>(async page =>
            {
                await page.ClickAsync("[data-testid='nav-todos']").ConfigureAwait(false);
            });
        }

        public TodosPage AddTodo(string text, string priority = "medium")
        {
            return Enqueue<IPage>(async page =>
            {
                await page.FillAsync("[data-testid='new-todo-input']", text).ConfigureAwait(false);
                await page.SelectOptionAsync("[data-testid='priority-select']", priority).ConfigureAwait(false);
                await page.ClickAsync("[data-testid='add-todo-btn']").ConfigureAwait(false);
            });
        }

        public TodosPage VerifyTodoVisible(string text)
        {
            return Enqueue<IPage>(async page =>
            {
                await page.Locator($"[data-testid='todo-text']:has-text('{text}')").WaitForAsync().ConfigureAwait(false);
            });
        }

        public TodosPage VerifyEmptyState()
        {
            return Enqueue<IPage>(async page =>
            {
                await page.Locator("[data-testid='empty-todo-list']").WaitForAsync().ConfigureAwait(false);
            });
        }

        public TodosPage CompleteTodo(int index)
        {
            return Enqueue<IPage>(async page =>
            {
                await page.Locator("[data-testid='todo-checkbox']").Nth(index).ClickAsync().ConfigureAwait(false);
            });
        }

        public TodosPage DeleteTodo(int index)
        {
            return Enqueue<IPage>(async page =>
            {
                await page.Locator("[data-testid='delete-todo-btn']").Nth(index).ClickAsync().ConfigureAwait(false);
            });
        }

        public TodosPage FilterBy(string filter)
        {
            return Enqueue<IPage>(async page =>
            {
                await page.SelectOptionAsync("[data-testid='filter-select']", filter).ConfigureAwait(false);
            });
        }

        public TodosPage ClearCompleted()
        {
            return Enqueue<IPage>(async page =>
            {
                await page.ClickAsync("[data-testid='clear-completed-btn']").ConfigureAwait(false);
            });
        }
    }
}
