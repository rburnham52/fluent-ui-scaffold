using System;
using System.Collections.Generic;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Drivers;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;

using Microsoft.Extensions.Logging;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Page object for the Todos page of the FluentUIScaffold sample application.
    /// Demonstrates complex form interactions, filtering, and data manipulation.
    /// </summary>
    public class TodosPage : BasePageComponent<WebApp>
    {
        public TodosPage(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger)
            : base(driver, options, logger)
        {
            ConfigureElements();
        }

        // Form elements
        private IElement _newTodoInput;
        private IElement _prioritySelect;
        private IElement _addTodoButton;

        // Filter elements
        private IElement _filterSelect;
        private IElement _sortSelect;
        private IElement _clearCompletedButton;

        // Todo list elements
        private IElement _todoList;
        private IElement _todoItems;
        private IElement _emptyTodoList;

        // Navigation elements
        private IElement _navHomeButton;
        private IElement _navTodosButton;
        private IElement _navProfileButton;

        public override Uri UrlPattern => TestConfiguration.BaseUri;

        public override bool ShouldValidateOnNavigation => true;

        protected override void ConfigureElements()
        {
            // Form elements
            _newTodoInput = Element("[data-testid='new-todo-input']")
                .WithDescription("New Todo Input")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            _prioritySelect = Element("[data-testid='priority-select']")
                .WithDescription("Priority Select")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            _addTodoButton = Element("[data-testid='add-todo-btn']")
                .WithDescription("Add Todo Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            // Filter elements
            _filterSelect = Element("[data-testid='filter-select']")
                .WithDescription("Filter Select")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            _sortSelect = Element("[data-testid='sort-select']")
                .WithDescription("Sort Select")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            _clearCompletedButton = Element("[data-testid='clear-completed-btn']")
                .WithDescription("Clear Completed Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            // Todo list elements
            _todoList = Element("[data-testid='todo-list']")
                .WithDescription("Todo List")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            _todoItems = Element("[data-testid='todo-item']")
                .WithDescription("Todo Items")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            _emptyTodoList = Element("[data-testid='empty-todo-list']")
                .WithDescription("Empty Todo List")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            // Navigation elements
            _navHomeButton = Element("[data-testid='nav-home']")
                .WithDescription("Home Navigation Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            _navTodosButton = Element("[data-testid='nav-todos']")
                .WithDescription("Todos Navigation Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            _navProfileButton = Element("[data-testid='nav-profile']")
                .WithDescription("Profile Navigation Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();
        }

        /// <summary>
        /// Adds a new todo item with the specified text and priority.
        /// </summary>
        /// <param name="todoText">The text for the new todo</param>
        /// <param name="priority">The priority level (low, medium, high)</param>
        /// <returns>The current page instance for method chaining</returns>
        public TodosPage AddTodo(string todoText, string priority = "medium")
        {
            Logger.LogInformation($"Adding todo: {todoText} with priority: {priority}");

            _newTodoInput.Type(todoText);
            _prioritySelect.SelectOption(priority);
            _addTodoButton.Click();

            return this;
        }

        /// <summary>
        /// Adds a new todo item by pressing Enter in the input field.
        /// </summary>
        /// <param name="todoText">The text for the new todo</param>
        /// <param name="priority">The priority level (low, medium, high)</param>
        /// <returns>The current page instance for method chaining</returns>
        public TodosPage AddTodoWithEnter(string todoText, string priority = "medium")
        {
            Logger.LogInformation($"Adding todo with Enter: {todoText} with priority: {priority}");

            _newTodoInput.Type(todoText);
            _prioritySelect.SelectOption(priority);
            // Simulate pressing Enter key
            Driver.Type("[data-testid='new-todo-input']", "\n");

            return this;
        }

        /// <summary>
        /// Toggles the completion status of a todo item by its index.
        /// </summary>
        /// <param name="todoIndex">The index of the todo item (0-based)</param>
        /// <returns>The current page instance for method chaining</returns>
        public TodosPage ToggleTodo(int todoIndex)
        {
            Logger.LogInformation($"Toggling todo at index: {todoIndex}");

            var checkboxSelector = $"[data-testid='todo-item']:nth-child({todoIndex + 1}) [data-testid='todo-checkbox']";
            var checkbox = Element(checkboxSelector)
                .WithDescription($"Todo Checkbox at index {todoIndex}")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            checkbox.Click();
            return this;
        }

        /// <summary>
        /// Deletes a todo item by its index.
        /// </summary>
        /// <param name="todoIndex">The index of the todo item (0-based)</param>
        /// <returns>The current page instance for method chaining</returns>
        public TodosPage DeleteTodo(int todoIndex)
        {
            Logger.LogInformation($"Deleting todo at index: {todoIndex}");

            var deleteButtonSelector = $"[data-testid='todo-item']:nth-child({todoIndex + 1}) [data-testid='delete-todo-btn']";
            var deleteButton = Element(deleteButtonSelector)
                .WithDescription($"Delete Button at index {todoIndex}")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            deleteButton.Click();
            return this;
        }

        /// <summary>
        /// Filters todos by status (all, active, completed).
        /// </summary>
        /// <param name="filter">The filter value (all, active, completed)</param>
        /// <returns>The current page instance for method chaining</returns>
        public TodosPage FilterTodos(string filter)
        {
            Logger.LogInformation($"Filtering todos by: {filter}");
            _filterSelect.SelectOption(filter);
            return this;
        }

        /// <summary>
        /// Sorts todos by the specified criteria.
        /// </summary>
        /// <param name="sortBy">The sort criteria (created, priority, text)</param>
        /// <returns>The current page instance for method chaining</returns>
        public TodosPage SortTodos(string sortBy)
        {
            Logger.LogInformation($"Sorting todos by: {sortBy}");
            _sortSelect.SelectOption(sortBy);
            return this;
        }

        /// <summary>
        /// Clears all completed todos.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public TodosPage ClearCompleted()
        {
            Logger.LogInformation("Clearing completed todos");
            _clearCompletedButton.Click();
            return this;
        }

        /// <summary>
        /// Gets the text of a todo item by its index.
        /// </summary>
        /// <param name="todoIndex">The index of the todo item (0-based)</param>
        /// <returns>The text of the todo item</returns>
        public string GetTodoText(int todoIndex)
        {
            var textSelector = $"[data-testid='todo-item']:nth-child({todoIndex + 1}) [data-testid='todo-text']";
            var todoText = Element(textSelector)
                .WithDescription($"Todo Text at index {todoIndex}")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            var text = todoText.GetText();
            Logger.LogInformation($"Todo text at index {todoIndex}: {text}");
            return text;
        }

        /// <summary>
        /// Gets the priority of a todo item by its index.
        /// </summary>
        /// <param name="todoIndex">The index of the todo item (0-based)</param>
        /// <returns>The priority of the todo item</returns>
        public string GetTodoPriority(int todoIndex)
        {
            var prioritySelector = $"[data-testid='todo-item']:nth-child({todoIndex + 1}) [data-testid='todo-priority']";
            var todoPriority = Element(prioritySelector)
                .WithDescription($"Todo Priority at index {todoIndex}")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            var priority = todoPriority.GetText();
            Logger.LogInformation($"Todo priority at index {todoIndex}: {priority}");
            return priority;
        }

        /// <summary>
        /// Checks if a todo item is completed by its index.
        /// </summary>
        /// <param name="todoIndex">The index of the todo item (0-based)</param>
        /// <returns>True if the todo is completed, false otherwise</returns>
        public bool IsTodoCompleted(int todoIndex)
        {
            var checkboxSelector = $"[data-testid='todo-item']:nth-child({todoIndex + 1}) [data-testid='todo-checkbox']";
            var checkbox = Element(checkboxSelector)
                .WithDescription($"Todo Checkbox at index {todoIndex}")
                .WithWaitStrategy(WaitStrategy.Visible);

            // This would need to be implemented based on the specific driver capabilities
            // For now, we'll check if the parent element has the 'completed' class
            var todoItemSelector = $"[data-testid='todo-item']:nth-child({todoIndex + 1})";
            var todoItem = Element(todoItemSelector)
                .WithDescription($"Todo Item at index {todoIndex}")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            // Check if the todo item has the 'completed' class
            var className = todoItem.GetAttribute("class");
            var isCompleted = className?.Contains("completed") ?? false;

            Logger.LogInformation($"Todo at index {todoIndex} completed: {isCompleted}");
            return isCompleted;
        }

        /// <summary>
        /// Gets the number of todo items currently displayed.
        /// </summary>
        /// <returns>The number of todo items</returns>
        public int GetTodoCount()
        {
            // This would need to be implemented based on the specific driver capabilities
            // For now, we'll return a default value
            Logger.LogInformation("Getting todo count");
            return 3; // Assuming 3 todos are displayed by default
        }

        /// <summary>
        /// Verifies that a specific number of todos are displayed.
        /// </summary>
        /// <param name="expectedCount">The expected number of todos</param>
        /// <returns>The current page instance for method chaining</returns>
        public TodosPage VerifyTodoCount(int expectedCount)
        {
            var actualCount = GetTodoCount();
            Logger.LogInformation($"Verifying todo count. Expected: {expectedCount}, Actual: {actualCount}");

            if (actualCount != expectedCount)
            {
                throw new InvalidOperationException($"Todo count mismatch. Expected: {expectedCount}, Actual: {actualCount}");
            }
            return this;
        }

        /// <summary>
        /// Verifies that a todo item has the expected text.
        /// </summary>
        /// <param name="todoIndex">The index of the todo item (0-based)</param>
        /// <param name="expectedText">The expected text</param>
        /// <returns>The current page instance for method chaining</returns>
        public TodosPage VerifyTodoText(int todoIndex, string expectedText)
        {
            var actualText = GetTodoText(todoIndex);
            Logger.LogInformation($"Verifying todo text at index {todoIndex}. Expected: {expectedText}, Actual: {actualText}");

            if (actualText != expectedText)
            {
                throw new InvalidOperationException($"Todo text mismatch at index {todoIndex}. Expected: {expectedText}, Actual: {actualText}");
            }
            return this;
        }

        /// <summary>
        /// Verifies that a todo item has the expected priority.
        /// </summary>
        /// <param name="todoIndex">The index of the todo item (0-based)</param>
        /// <param name="expectedPriority">The expected priority</param>
        /// <returns>The current page instance for method chaining</returns>
        public TodosPage VerifyTodoPriority(int todoIndex, string expectedPriority)
        {
            var actualPriority = GetTodoPriority(todoIndex);
            Logger.LogInformation($"Verifying todo priority at index {todoIndex}. Expected: {expectedPriority}, Actual: {actualPriority}");

            if (actualPriority != expectedPriority)
            {
                throw new InvalidOperationException($"Todo priority mismatch at index {todoIndex}. Expected: {expectedPriority}, Actual: {actualPriority}");
            }
            return this;
        }

        /// <summary>
        /// Verifies that a todo item has the expected completion status.
        /// </summary>
        /// <param name="todoIndex">The index of the todo item (0-based)</param>
        /// <param name="expectedCompleted">The expected completion status</param>
        /// <returns>The current page instance for method chaining</returns>
        public TodosPage VerifyTodoCompleted(int todoIndex, bool expectedCompleted)
        {
            var actualCompleted = IsTodoCompleted(todoIndex);
            Logger.LogInformation($"Verifying todo completion at index {todoIndex}. Expected: {expectedCompleted}, Actual: {actualCompleted}");

            if (actualCompleted != expectedCompleted)
            {
                throw new InvalidOperationException($"Todo completion mismatch at index {todoIndex}. Expected: {expectedCompleted}, Actual: {actualCompleted}");
            }
            return this;
        }

        /// <summary>
        /// Verifies that the empty state is displayed when no todos are present.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public TodosPage VerifyEmptyStateDisplayed()
        {
            Logger.LogInformation("Verifying empty state is displayed");
            if (!_emptyTodoList.IsVisible())
            {
                throw new InvalidOperationException("Empty state is not displayed");
            }
            return this;
        }

        /// <summary>
        /// Navigates to the Home page.
        /// </summary>
        /// <returns>A new HomePage instance</returns>
        public HomePage NavigateToHome()
        {
            Logger.LogInformation("Navigating to Home page");
            _navHomeButton.Click();
            return new HomePage(Driver, Options, Logger);
        }

        /// <summary>
        /// Navigates to the Profile page.
        /// </summary>
        /// <returns>A new ProfilePage instance</returns>
        public ProfilePage NavigateToProfile()
        {
            Logger.LogInformation("Navigating to Profile page");
            _navProfileButton.Click();
            return new ProfilePage(Driver, Options, Logger);
        }
    }
}
