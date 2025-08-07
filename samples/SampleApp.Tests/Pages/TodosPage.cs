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
            // Wait for the todos section to be loaded
            Driver.WaitForElementToBeVisible(".todos-section");
            Driver.Type("[data-testid='new-todo-input']", todoText);
            Driver.Click("[data-testid='add-todo-btn']");
            return this;
        }

        public TodosPage CompleteTodo(int index)
        {
            Driver.Click($"[data-testid='todo-checkbox']:nth-child({index + 1})");
            return this;
        }

        public TodosPage DeleteTodo(int index)
        {
            Driver.Click($"[data-testid='delete-todo-btn']:nth-child({index + 1})");
            return this;
        }

        public string GetTodoText(int index)
        {
            return Driver.GetText($"[data-testid='todo-text']:nth-child({index + 1})");
        }

        public bool IsTodoCompleted(int index)
        {
            return Driver.IsVisible($"[data-testid='todo-item']:nth-child({index + 1}).completed");
        }
    }
}
