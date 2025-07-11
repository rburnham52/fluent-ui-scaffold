# Story 3.2.1: Sample Web Application (MVP Focus)

## Story Information
- **Epic**: Epic 3.2 - Sample Applications and Integration Tests
- **Priority**: High (MVP Focus)
- **Estimated Time**: 1-2 weeks
- **Status**: 🔴 Not Started
- **Assigned To**: TBD
- **Dependencies**: Story 1.2.1, Story 1.3.1 (minimal)
- **File**: `phase-3-documentation/epic-3.2-samples/story-3.2.1-sample-web-app.md`

## User Story

**As a** test developer  
**I want** a simple sample web application with example tests that demonstrate the framework's core capabilities  
**So that** I can quickly understand how to use FluentUIScaffold and validate the framework design

## Acceptance Criteria

- [ ] Simple web application is created (Todo App or similar)
- [ ] Web app includes common UI patterns (forms, lists, navigation)
- [ ] Sample page objects are implemented using the framework
- [ ] Example tests demonstrate core framework features
- [ ] Tests cover basic scenarios (navigation, form submission, verification)
- [ ] Tests are readable and serve as documentation
- [ ] Web app can be easily started for testing
- [ ] Tests can be run against the sample app
- [ ] Framework demonstrates fluent API usage
- [ ] Example shows best practices and patterns

## Technical Tasks

### 1. Create Simple Web Application

```html
<!-- Simple Todo App - index.html -->
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>FluentUIScaffold Sample App</title>
    <style>
        body { font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; }
        .todo-item { display: flex; align-items: center; margin: 10px 0; padding: 10px; border: 1px solid #ddd; }
        .todo-item.completed { background-color: #f0f0f0; text-decoration: line-through; }
        .todo-text { flex: 1; margin: 0 10px; }
        .delete-btn { background: #ff4444; color: white; border: none; padding: 5px 10px; cursor: pointer; }
        .add-form { margin: 20px 0; padding: 20px; border: 1px solid #ddd; }
        .add-form input { width: 70%; padding: 8px; margin-right: 10px; }
        .add-form button { padding: 8px 16px; background: #4CAF50; color: white; border: none; cursor: pointer; }
        .stats { margin: 20px 0; padding: 10px; background: #f9f9f9; }
    </style>
</head>
<body>
    <h1>FluentUIScaffold Sample Todo App</h1>
    
    <div class="add-form">
        <input type="text" id="new-todo" placeholder="Enter new todo item">
        <button id="add-todo">Add Todo</button>
    </div>
    
    <div class="stats">
        <span id="total-count">Total: 0</span> | 
        <span id="completed-count">Completed: 0</span> | 
        <span id="pending-count">Pending: 0</span>
    </div>
    
    <div id="todo-list">
        <!-- Todo items will be added here -->
    </div>

    <script>
        let todos = [];
        let nextId = 1;

        function updateStats() {
            const total = todos.length;
            const completed = todos.filter(t => t.completed).length;
            const pending = total - completed;
            
            document.getElementById('total-count').textContent = `Total: ${total}`;
            document.getElementById('completed-count').textContent = `Completed: ${completed}`;
            document.getElementById('pending-count').textContent = `Pending: ${pending}`;
        }

        function renderTodos() {
            const list = document.getElementById('todo-list');
            list.innerHTML = '';
            
            todos.forEach(todo => {
                const item = document.createElement('div');
                item.className = `todo-item ${todo.completed ? 'completed' : ''}`;
                item.innerHTML = `
                    <input type="checkbox" ${todo.completed ? 'checked' : ''} 
                           onchange="toggleTodo(${todo.id})">
                    <span class="todo-text">${todo.text}</span>
                    <button class="delete-btn" onclick="deleteTodo(${todo.id})">Delete</button>
                `;
                list.appendChild(item);
            });
            
            updateStats();
        }

        function addTodo() {
            const input = document.getElementById('new-todo');
            const text = input.value.trim();
            
            if (text) {
                todos.push({ id: nextId++, text, completed: false });
                input.value = '';
                renderTodos();
            }
        }

        function toggleTodo(id) {
            const todo = todos.find(t => t.id === id);
            if (todo) {
                todo.completed = !todo.completed;
                renderTodos();
            }
        }

        function deleteTodo(id) {
            todos = todos.filter(t => t.id !== id);
            renderTodos();
        }

        // Event listeners
        document.getElementById('add-todo').addEventListener('click', addTodo);
        document.getElementById('new-todo').addEventListener('keypress', (e) => {
            if (e.key === 'Enter') addTodo();
        });

        // Initialize
        renderTodos();
    </script>
</body>
</html>
```

### 2. Create Sample Page Objects

```csharp
// TodoAppPage.cs
public class TodoAppPage : BasePageComponent<WebApp>
{
    public override string UrlPattern => "/";
    
    // Elements
    protected IElement NewTodoInput => Element("#new-todo");
    protected IElement AddTodoButton => Element("#add-todo");
    protected IElement TodoList => Element("#todo-list");
    protected IElement TotalCount => Element("#total-count");
    protected IElement CompletedCount => Element("#completed-count");
    protected IElement PendingCount => Element("#pending-count");
    
    protected override void ConfigureElements()
    {
        // Elements are configured in properties above
    }
    
    // Actions
    public TodoAppPage AddTodo(string todoText)
    {
        Logger.LogInformation($"Adding todo: {todoText}");
        NewTodoInput.Type(todoText);
        AddTodoButton.Click();
        return this;
    }
    
    public TodoAppPage ToggleTodo(int todoId)
    {
        Logger.LogInformation($"Toggling todo {todoId}");
        var checkbox = Element($"input[onchange*='toggleTodo({todoId})']");
        checkbox.Click();
        return this;
    }
    
    public TodoAppPage DeleteTodo(int todoId)
    {
        Logger.LogInformation($"Deleting todo {todoId}");
        var deleteButton = Element($"button[onclick*='deleteTodo({todoId})']");
        deleteButton.Click();
        return this;
    }
    
    public TodoAppPage VerifyTodoExists(string todoText)
    {
        Verify.ElementContainsText("#todo-list", todoText);
        return this;
    }
    
    public TodoAppPage VerifyTodoCompleted(string todoText)
    {
        Verify.That(() => {
            var todoItems = Driver.GetElements(".todo-item.completed");
            return todoItems.Any(item => item.Text.Contains(todoText));
        }, $"Todo '{todoText}' should be completed");
        return this;
    }
    
    public TodoAppPage VerifyStats(int total, int completed, int pending)
    {
        Verify.ElementContainsText("#total-count", $"Total: {total}");
        Verify.ElementContainsText("#completed-count", $"Completed: {completed}");
        Verify.ElementContainsText("#pending-count", $"Pending: {pending}");
        return this;
    }
}
```

### 3. Create Example Tests

```csharp
// TodoAppTests.cs
[TestFixture]
public class TodoAppTests
{
    private FluentUIScaffoldApp<WebApp> _scaffold;
    
    [SetUp]
    public void Setup()
    {
        _scaffold = FluentUIScaffold<WebApp>.Web(options =>
        {
            options.BaseUrl = new Uri("http://localhost:5000");
            options.DefaultTimeout = TimeSpan.FromSeconds(10);
            options.HeadlessMode = false; // For demo purposes
        });
    }
    
    [TearDown]
    public void Teardown()
    {
        _scaffold?.Dispose();
    }
    
    [Test]
    public void Can_Add_New_Todo()
    {
        // Arrange
        var todoText = "Learn FluentUIScaffold";
        
        // Act
        var page = _scaffold
            .NavigateToUrl(new Uri("http://localhost:5000"))
            .GetPage<TodoAppPage>();
        
        page.AddTodo(todoText);
        
        // Assert
        page.VerifyTodoExists(todoText)
            .VerifyStats(1, 0, 1);
    }
    
    [Test]
    public void Can_Complete_Todo()
    {
        // Arrange
        var todoText = "Complete this task";
        
        // Act
        var page = _scaffold
            .NavigateToUrl(new Uri("http://localhost:5000"))
            .GetPage<TodoAppPage>();
        
        page.AddTodo(todoText);
        
        // Find the first todo and toggle it
        var firstTodoCheckbox = page.Driver.GetElement(".todo-item input[type='checkbox']");
        firstTodoCheckbox.Click();
        
        // Assert
        page.VerifyTodoCompleted(todoText)
            .VerifyStats(1, 1, 0);
    }
    
    [Test]
    public void Can_Delete_Todo()
    {
        // Arrange
        var todoText = "Delete this task";
        
        // Act
        var page = _scaffold
            .NavigateToUrl(new Uri("http://localhost:5000"))
            .GetPage<TodoAppPage>();
        
        page.AddTodo(todoText);
        page.DeleteTodo(1); // Delete the first todo
        
        // Assert
        page.Verify.That(() => !page.Driver.GetText("#todo-list").Contains(todoText))
            .VerifyStats(0, 0, 0);
    }
    
    [Test]
    public void Can_Add_Multiple_Todos()
    {
        // Arrange
        var todos = new[] { "First todo", "Second todo", "Third todo" };
        
        // Act
        var page = _scaffold
            .NavigateToUrl(new Uri("http://localhost:5000"))
            .GetPage<TodoAppPage>();
        
        foreach (var todo in todos)
        {
            page.AddTodo(todo);
        }
        
        // Assert
        foreach (var todo in todos)
        {
            page.VerifyTodoExists(todo);
        }
        page.VerifyStats(3, 0, 3);
    }
    
    [Test]
    public void Can_Complete_Multiple_Todos()
    {
        // Arrange
        var todos = new[] { "Todo 1", "Todo 2", "Todo 3" };
        
        // Act
        var page = _scaffold
            .NavigateToUrl(new Uri("http://localhost:5000"))
            .GetPage<TodoAppPage>();
        
        // Add todos
        foreach (var todo in todos)
        {
            page.AddTodo(todo);
        }
        
        // Complete first two todos
        page.ToggleTodo(1);
        page.ToggleTodo(2);
        
        // Assert
        page.VerifyTodoCompleted("Todo 1")
            .VerifyTodoCompleted("Todo 2")
            .VerifyStats(3, 2, 1);
    }
}
```

### 4. Create Simple Web Server

```csharp
// SimpleWebServer.cs
public class SimpleWebServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly string _contentPath;
    private bool _isRunning;
    
    public SimpleWebServer(string url, string contentPath)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(url);
        _contentPath = contentPath;
    }
    
    public void Start()
    {
        _listener.Start();
        _isRunning = true;
        
        Task.Run(async () =>
        {
            while (_isRunning)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    await HandleRequest(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling request: {ex.Message}");
                }
            }
        });
    }
    
    private async Task HandleRequest(HttpListenerContext context)
    {
        var response = context.Response;
        var path = context.Request.Url.LocalPath;
        
        if (path == "/" || path == "/index.html")
        {
            var htmlPath = Path.Combine(_contentPath, "index.html");
            if (File.Exists(htmlPath))
            {
                var content = await File.ReadAllTextAsync(htmlPath);
                var buffer = Encoding.UTF8.GetBytes(content);
                
                response.ContentType = "text/html";
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
        
        response.Close();
    }
    
    public void Dispose()
    {
        _isRunning = false;
        _listener?.Stop();
        _listener?.Close();
    }
}
```

### 5. Create Test Setup Helper

```csharp
// TestSetup.cs
public static class TestSetup
{
    private static SimpleWebServer _webServer;
    
    [OneTimeSetUp]
    public static void StartWebServer()
    {
        var contentPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "SampleApp");
        _webServer = new SimpleWebServer("http://localhost:5000/", contentPath);
        _webServer.Start();
        
        // Wait a moment for server to start
        Thread.Sleep(1000);
    }
    
    [OneTimeTearDown]
    public static void StopWebServer()
    {
        _webServer?.Dispose();
    }
}
```

### 6. Create Framework Integration Example

```csharp
// FrameworkUsageExamples.cs
[TestFixture]
public class FrameworkUsageExamples
{
    [Test]
    public void Basic_Framework_Usage_Example()
    {
        // Basic configuration
        var scaffold = FluentUIScaffold<WebApp>.Web(options =>
        {
            options.BaseUrl = new Uri("http://localhost:5000");
            options.DefaultTimeout = TimeSpan.FromSeconds(10);
            options.HeadlessMode = false;
        });
        
        // Navigate and interact
        scaffold
            .NavigateToUrl(new Uri("http://localhost:5000"))
            .GetPage<TodoAppPage>()
            .AddTodo("Test todo")
            .VerifyTodoExists("Test todo");
    }
    
    [Test]
    public void Advanced_Framework_Usage_Example()
    {
        var scaffold = FluentUIScaffold<WebApp>.Web(options =>
        {
            options.BaseUrl = new Uri("http://localhost:5000");
            options.DefaultTimeout = TimeSpan.FromSeconds(10);
            options.CaptureScreenshotsOnFailure = true;
        });
        
        // Chain multiple actions
        var page = scaffold
            .NavigateToUrl(new Uri("http://localhost:5000"))
            .GetPage<TodoAppPage>();
        
        // Add multiple todos
        page.AddTodo("First todo")
            .AddTodo("Second todo")
            .AddTodo("Third todo");
        
        // Complete some todos
        page.ToggleTodo(1)
            .ToggleTodo(2);
        
        // Verify final state
        page.VerifyStats(3, 2, 1)
            .VerifyTodoCompleted("First todo")
            .VerifyTodoCompleted("Second todo");
    }
}
```

## Definition of Done

- [ ] Simple web application is created and functional
- [ ] Sample page objects are implemented using the framework
- [ ] Example tests demonstrate core framework features
- [ ] Tests cover basic scenarios (navigation, form submission, verification)
- [ ] Tests are readable and serve as documentation
- [ ] Web app can be easily started for testing
- [ ] Tests can be run against the sample app
- [ ] Framework demonstrates fluent API usage
- [ ] Example shows best practices and patterns
- [ ] Documentation explains how to run the sample
- [ ] Sample is included in the project structure
- [ ] Tests pass consistently
- [ ] Story status is updated to "Completed"

## Notes

- This MVP focuses on demonstrating core framework capabilities
- The sample web app should be simple but realistic
- Tests should be educational and show best practices
- Consider this as a foundation for more complex examples later
- The sample should be easy to run and understand 