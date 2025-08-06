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
            Driver.Type("#todo-input", todoText);
            Driver.Click("#add-todo-button");
            return this;
        }

        public TodosPage CompleteTodo(int index)
        {
            Driver.Click($"#todo-{index}-checkbox");
            return this;
        }

        public TodosPage DeleteTodo(int index)
        {
            Driver.Click($"#todo-{index}-delete");
            return this;
        }

        public string GetTodoText(int index)
        {
            return Driver.GetText($"#todo-{index}-text");
        }

        public bool IsTodoCompleted(int index)
        {
            return Driver.IsVisible($"#todo-{index}-completed");
        }
    }
}
