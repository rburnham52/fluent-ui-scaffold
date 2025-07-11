<script lang="ts">
  interface Todo {
    id: number;
    text: string;
    completed: boolean;
    priority: 'low' | 'medium' | 'high';
    createdAt: Date;
  }

  let todos: Todo[] = [
    { id: 1, text: 'Learn FluentUIScaffold', completed: false, priority: 'high', createdAt: new Date() },
    { id: 2, text: 'Write E2E tests', completed: false, priority: 'medium', createdAt: new Date() },
    { id: 3, text: 'Document the framework', completed: true, priority: 'low', createdAt: new Date() }
  ];

  let newTodoText = '';
  let selectedPriority: 'low' | 'medium' | 'high' = 'medium';
  let filterStatus: 'all' | 'active' | 'completed' = 'all';
  let sortBy: 'created' | 'priority' | 'text' = 'created';

  function addTodo() {
    if (newTodoText.trim()) {
      const todo: Todo = {
        id: Date.now(),
        text: newTodoText.trim(),
        completed: false,
        priority: selectedPriority,
        createdAt: new Date()
      };
      todos = [...todos, todo];
      newTodoText = '';
    }
  }

  function toggleTodo(id: number) {
    todos = todos.map(todo => 
      todo.id === id ? { ...todo, completed: !todo.completed } : todo
    );
  }

  function deleteTodo(id: number) {
    todos = todos.filter(todo => todo.id !== id);
  }

  function updateTodoText(id: number, newText: string) {
    todos = todos.map(todo => 
      todo.id === id ? { ...todo, text: newText } : todo
    );
  }

  function clearCompleted() {
    todos = todos.filter(todo => !todo.completed);
  }

  $: filteredTodos = todos.filter(todo => {
    if (filterStatus === 'active') return !todo.completed;
    if (filterStatus === 'completed') return todo.completed;
    return true;
  });

  $: sortedTodos = [...filteredTodos].sort((a, b) => {
    switch (sortBy) {
      case 'priority':
        const priorityOrder = { high: 3, medium: 2, low: 1 };
        return priorityOrder[b.priority] - priorityOrder[a.priority];
      case 'text':
        return a.text.localeCompare(b.text);
      default:
        return b.createdAt.getTime() - a.createdAt.getTime();
    }
  });

  $: completedCount = todos.filter(t => t.completed).length;
  $: totalCount = todos.length;
</script>

<div class="todo-container">
  <header class="todo-header">
    <h2>Todo List</h2>
    <p class="todo-stats">
      {completedCount} of {totalCount} completed
    </p>
  </header>

  <div class="todo-controls">
    <div class="add-todo">
      <input
        type="text"
        bind:value={newTodoText}
        placeholder="Add a new todo..."
        data-testid="new-todo-input"
        on:keydown={(e) => e.key === 'Enter' && addTodo()}
        class="todo-input"
      />
      <select bind:value={selectedPriority} data-testid="priority-select" class="priority-select">
        <option value="low">Low</option>
        <option value="medium">Medium</option>
        <option value="high">High</option>
      </select>
      <button on:click={addTodo} data-testid="add-todo-btn" class="add-btn">
        Add Todo
      </button>
    </div>

    <div class="todo-filters-row">
      <div class="filter-group">
        <label>Filter:</label>
        <select bind:value={filterStatus} data-testid="filter-select" class="filter-select">
          <option value="all">All</option>
          <option value="active">Active</option>
          <option value="completed">Completed</option>
        </select>
      </div>

      <div class="filter-group">
        <label>Sort by:</label>
        <select bind:value={sortBy} data-testid="sort-select" class="sort-select">
          <option value="created">Created Date</option>
          <option value="priority">Priority</option>
          <option value="text">Text</option>
        </select>
      </div>

      <button 
        on:click={clearCompleted} 
        data-testid="clear-completed-btn"
        class="clear-btn"
        disabled={completedCount === 0}
      >
        Clear Completed
      </button>
    </div>
  </div>

  <ul class="todo-list" data-testid="todo-list">
    {#each sortedTodos as todo (todo.id)}
      <li class="todo-item {todo.completed ? 'completed' : ''}" data-testid="todo-item">
        <div class="todo-content">
          <input
            type="checkbox"
            checked={todo.completed}
            on:change={() => toggleTodo(todo.id)}
            data-testid="todo-checkbox"
            class="todo-checkbox"
            style="appearance: none; -webkit-appearance: none; background: #fff; border: 1.5px solid #e3e8ee; border-radius: 0.25rem; width: 1.2rem; height: 1.2rem; vertical-align: middle; position: relative; cursor: pointer; outline: none; transition: border 0.2s; accent-color: #007bff; display: inline-block; margin-right: 0.5rem;"
          />
          <span 
            class="todo-text {todo.completed ? 'completed' : ''}"
            data-testid="todo-text"
          >
            {todo.text}
          </span>
        </div>
        <div class="todo-actions">
          <span class="todo-priority badge badge-{todo.priority}" data-testid="todo-priority">
            {todo.priority}
          </span>
          <button 
            on:click={() => deleteTodo(todo.id)}
            data-testid="delete-todo-btn"
            class="delete-btn"
          >
            Delete
          </button>
        </div>
      </li>
    {/each}
  </ul>

  {#if sortedTodos.length === 0}
    <div class="empty-state" data-testid="empty-todo-list">
      <p>No todos found. Add one above!</p>
    </div>
  {/if}
</div>

<style>
  .todo-container {
    max-width: 600px;
    margin: 0 auto;
    background: transparent;
  }
  .todo-header {
    text-align: center;
    margin-bottom: 2rem;
  }
  .todo-header h2 {
    font-size: 1.5rem;
    font-weight: 700;
    margin-bottom: 0.2rem;
    color: #23272f;
  }
  .todo-stats {
    color: #666;
    font-size: 1rem;
    margin-bottom: 0.5rem;
  }
  .todo-controls {
    margin-bottom: 2rem;
    display: flex;
    flex-direction: column;
    gap: 1rem;
  }
  .add-todo {
    display: flex;
    gap: 0.5rem;
    margin-bottom: 0.5rem;
    align-items: center;
  }
  .todo-input,
  .priority-select,
  .filter-select,
  .sort-select {
    background: #fff !important;
    color: #23272f !important;
    border: 1.5px solid #e3e8ee;
    border-radius: 0.375rem;
    font-size: 1rem;
    font-weight: 500;
    outline: none;
    box-shadow: 0 1px 4px rgba(0,0,0,0.07);
    transition: border 0.2s, box-shadow 0.2s;
}
.todo-input:focus,
.priority-select:focus,
.filter-select:focus,
.sort-select:focus {
    border: 1.5px solid #007bff;
    box-shadow: 0 2px 8px #007bff33;
}
.todo-input::placeholder {
    color: #b0b8c1;
    opacity: 1;
}
.todo-checkbox {
    width: 1.2rem;
    height: 1.2rem;
    accent-color: #007bff;
    border-radius: 0.25rem;
    margin-right: 0.5rem;
    vertical-align: middle;
    background: #fff;
    border: 1.5px solid #e3e8ee;
    transition: border 0.2s;
}
.todo-checkbox:focus {
    border: 1.5px solid #007bff;
}
  .add-btn {
    padding: 0.7rem 1.5rem;
    background: #007bff;
    color: #fff;
    border: none;
    border-radius: 0.375rem;
    font-size: 1rem;
    font-weight: 600;
    cursor: pointer;
    box-shadow: 0 1px 4px rgba(0,123,255,0.08);
    transition: background 0.2s;
  }
  .add-btn:hover {
    background: #0056b3;
  }
  .todo-filters-row {
    display: flex;
    gap: 1rem;
    align-items: center;
    flex-wrap: wrap;
    margin-bottom: 0.5rem;
  }
  .filter-group {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    background: none;
  }
  .filter-select, .sort-select {
    padding: 0.5rem 1rem;
    border: none;
    border-radius: 0.375rem;
    background: #23272f;
    color: #fff;
    font-size: 1rem;
    font-weight: 500;
    outline: none;
  }
  .clear-btn {
    padding: 0.5rem 1.2rem;
    background: #dc3545;
    color: #fff;
    border: none;
    border-radius: 0.375rem;
    font-size: 1rem;
    font-weight: 600;
    cursor: pointer;
    transition: background 0.2s;
    margin-left: 0.5rem;
  }
  .clear-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
  .clear-btn:hover:not(:disabled) {
    background: #b52a37;
  }
  .todo-list {
    list-style: none;
    padding: 0;
    margin: 0;
    display: flex;
    flex-direction: column;
    gap: 1rem;
  }
  .todo-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1.1rem 1.5rem;
    border-radius: 0.75rem;
    background: #fff;
    box-shadow: 0 1px 8px rgba(0,0,0,0.07);
    font-size: 1.1rem;
    font-weight: 500;
    transition: background 0.2s, color 0.2s;
    border: 1px solid #e3e8ee;
    gap: 1rem;
  }
  .todo-item.completed {
    background: #f4f6fa;
    color: #aaa;
    text-decoration: none;
    opacity: 0.7;
  }
  .todo-content {
    display: flex;
    align-items: center;
    gap: 1rem;
    flex: 1;
  }
  .todo-checkbox {
    width: 1.2rem;
    height: 1.2rem;
    accent-color: #007bff;
    border-radius: 0.25rem;
    margin-right: 0.5rem;
    vertical-align: middle;
    background: #fff;
    border: 1.5px solid #e3e8ee;
    transition: border 0.2s;
}
.todo-checkbox:focus {
    border: 1.5px solid #007bff;
}
  .todo-text {
    flex: 1;
    font-size: 1.1rem;
    color: #23272f;
    font-weight: 500;
    transition: color 0.2s;
  }
  .todo-text.completed {
    text-decoration: line-through;
    color: #aaa;
    font-weight: 400;
  }
  .todo-actions {
    display: flex;
    align-items: center;
    gap: 0.7rem;
  }
  .badge {
    display: inline-block;
    padding: 0.3em 1em;
    border-radius: 1em;
    font-size: 0.95em;
    font-weight: 700;
    text-transform: lowercase;
    margin-right: 0.5em;
    letter-spacing: 0.01em;
    box-shadow: 0 1px 4px rgba(0,0,0,0.04);
  }
  .badge-high {
    background: #dc3545;
    color: #fff;
  }
  .badge-medium {
    background: #ffc107;
    color: #23272f;
  }
  .badge-low {
    background: #28a745;
    color: #fff;
  }
  .delete-btn {
    padding: 0.3em 1em;
    background: #dc3545;
    color: #fff;
    border: none;
    border-radius: 0.375em;
    font-size: 1em;
    font-weight: 600;
    cursor: pointer;
    transition: background 0.2s;
    box-shadow: 0 1px 4px rgba(0,0,0,0.04);
  }
  .delete-btn:hover {
    background: #b52a37;
  }
  .empty-state {
    text-align: center;
    padding: 2rem;
    color: #666;
    font-size: 1.1rem;
  }
  @media (max-width: 700px) {
    .todo-container {
      padding: 0 0.5rem;
    }
    .todo-header h2 {
      font-size: 1.1rem;
    }
    .todo-item {
      padding: 0.7rem 0.7rem;
      font-size: 1rem;
    }
    .add-todo, .todo-filters-row {
      flex-direction: column;
      align-items: stretch;
      gap: 0.5rem;
    }
    .todo-actions {
      gap: 0.3rem;
    }
  }
  .todo-checkbox:checked::before {
    content: '';
    display: block;
    width: 100%;
    height: 100%;
    background: #007bff;
    border-radius: 0.2rem;
    position: absolute;
    left: 0;
    top: 0;
  }
  .todo-checkbox:checked {
    background: #007bff;
    border-color: #007bff;
  }
  .todo-checkbox:focus {
    border: 1.5px solid #007bff;
    box-shadow: 0 0 0 2px #007bff33;
  }
</style> 