using System;
using System.Collections.Generic;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;
using FluentUIScaffold.Playwright;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Page object for the Todos page of the FluentUIScaffold sample application.
    /// Demonstrates complex form interactions, filtering, and data manipulation.
    /// </summary>
    public class TodosPage : BasePageComponent<PlaywrightDriver, TodosPage>
    {
        public TodosPage(IServiceProvider serviceProvider, Uri urlPattern)
            : base(serviceProvider, urlPattern)
        {
        }

        protected override void ConfigureElements()
        {
            // Configure elements for the todos page
        }

        public TodosPage AddTodo(string todoText)
        {
            // Ensure nav is present then go to todos
            if (!Driver.IsVisible(".todos-section"))
            {
                if (!Driver.IsVisible("nav"))
                {
                    Driver.NavigateToUrl(TestConfiguration.BaseUri);
                    Driver.WaitForElementToBeVisible("nav");
                }
                Driver.Click("nav button[data-testid='nav-todos']");
                Driver.WaitForElementToBeVisible(".todos-section");
            }
            Driver.WaitForElementToBeVisible("[data-testid='new-todo-input']");
            Driver.Type("[data-testid='new-todo-input']", todoText);
            Driver.Click("[data-testid='add-todo-btn']");
            // Wait for at least one todo item to appear
            Driver.WaitForElementToBeVisible("[data-testid='todo-item']");
            return this;
        }

        public TodosPage CompleteTodo(int index)
        {
            Driver.Click($"[data-testid='todo-item']:nth-of-type({index + 1}) [data-testid='todo-checkbox']");
            return this;
        }

        public TodosPage DeleteTodo(int index)
        {
            Driver.Click($"[data-testid='todo-item']:nth-of-type({index + 1}) [data-testid='delete-todo-btn']");
            return this;
        }

        public string GetTodoText(int index)
        {
            // Ensure list has at least index+1 items
            Driver.WaitForElementToBeVisible("[data-testid='todo-item']");
            return Driver.GetText($"[data-testid='todo-item']:nth-of-type({index + 1}) [data-testid='todo-text']");
        }

        public bool IsTodoCompleted(int index)
        {
            return Driver.IsVisible($"[data-testid='todo-item']:nth-of-type({index + 1}).completed");
        }
    }
}
