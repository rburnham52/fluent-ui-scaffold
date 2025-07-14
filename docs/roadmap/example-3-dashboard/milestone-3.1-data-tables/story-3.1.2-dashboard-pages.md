# Story 3.1.2: Create Dashboard Pages

## Overview

Create the DashboardPage and related pages with their corresponding sample app pages, implementing the complete dashboard functionality for Example 3.

## Background

Example 3 requires a complete dashboard system with data tables, user management, and complex interactions. The V2.0 specification shows:

```csharp
fluentUI
    .NavigateTo<DashboardPage>()
    .Click(e => e.UsersTableSortButton)
    .Click(e => e.UsersTableFilterButton)
    .Type(e => e.UsersTableSearchInput, "admin")
    .Click(e => e.UsersTableRow(2))
    .Click(e => e.UsersTableEditButton)
    .Type(e => e.UserNameInput, "newadmin")
    .Click(e => e.SaveButton)
    .Verify(e => e.StatusMessage.HasText("User updated successfully"))
    .Verify(e => e.UsersTableRow(2).ContainsText("newadmin"));
```

This story focuses on creating the page classes and sample app implementation.

## Acceptance Criteria

- [ ] Create DashboardPage with data table functionality
- [ ] Create UserEditModal with form interactions
- [ ] Add sample app pages for dashboard and user management
- [ ] Implement data table with sorting, filtering, and pagination

## Technical Requirements

### 1. Dashboard Page Implementation

Create a complete DashboardPage class:

```csharp
public class DashboardPage : BasePageComponent<PlaywrightDriver, DashboardPage>
{
    public IElement UsersTable { get; private set; }
    public IElement UsersTableSortButton { get; private set; }
    public IElement UsersTableFilterButton { get; private set; }
    public IElement UsersTableSearchInput { get; private set; }
    public IElement UsersTableRow { get; private set; }
    public IElement UsersTableEditButton { get; private set; }
    public IElement UsersTableDeleteButton { get; private set; }
    public IElement StatusMessage { get; private set; }
    public IElement AddUserButton { get; private set; }
    public IElement RefreshButton { get; private set; }
    public IElement DashboardTitle { get; private set; }
    public IElement UserStats { get; private set; }
    public IElement PaginationInfo { get; private set; }
    public IElement ExportButton { get; private set; }
    public IElement ImportButton { get; private set; }
    
    private DataTableCollection _usersTableCollection;
    
    public DashboardPage(IServiceProvider serviceProvider, Uri urlPattern) 
        : base(serviceProvider, urlPattern)
    {
    }
    
    protected override void ConfigureElements()
    {
        UsersTable = new Element("#users-table", "Users Table", ElementType.Table);
        UsersTableSortButton = new Element("#users-table-sort", "Users Table Sort Button", ElementType.Button);
        UsersTableFilterButton = new Element("#users-table-filter", "Users Table Filter Button", ElementType.Button);
        UsersTableSearchInput = new Element("#users-table-search", "Users Table Search Input", ElementType.Input);
        UsersTableRow = new Element("#users-table-row", "Users Table Row", ElementType.Tr);
        UsersTableEditButton = new Element("#users-table-edit", "Users Table Edit Button", ElementType.Button);
        UsersTableDeleteButton = new Element("#users-table-delete", "Users Table Delete Button", ElementType.Button);
        StatusMessage = new Element("#status-message", "Status Message", ElementType.Div);
        AddUserButton = new Element("#add-user-button", "Add User Button", ElementType.Button);
        RefreshButton = new Element("#refresh-button", "Refresh Button", ElementType.Button);
        DashboardTitle = new Element("#dashboard-title", "Dashboard Title", ElementType.H1);
        UserStats = new Element("#user-stats", "User Stats", ElementType.Div);
        PaginationInfo = new Element("#pagination-info", "Pagination Info", ElementType.Div);
        ExportButton = new Element("#export-button", "Export Button", ElementType.Button);
        ImportButton = new Element("#import-button", "Import Button", ElementType.Button);
        
        _usersTableCollection = new DataTableCollection(Driver, Options, "#users-table");
    }
    
    // Dashboard interaction methods
    public DashboardPage RefreshData()
    {
        return this.Click(e => e.RefreshButton);
    }
    
    public DashboardPage AddNewUser()
    {
        return this.Click(e => e.AddUserButton);
    }
    
    public DashboardPage ExportData()
    {
        return this.Click(e => e.ExportButton);
    }
    
    public DashboardPage ImportData()
    {
        return this.Click(e => e.ImportButton);
    }
    
    // Data table interaction methods
    public DashboardPage SortUsersTable(string columnName)
    {
        var sortButton = _usersTableCollection.GetSortButton(columnName);
        return this.Click(sortButton.Selector);
    }
    
    public DashboardPage FilterUsersTable(string columnName, string filterValue)
    {
        var filterButton = _usersTableCollection.GetFilterButton(columnName);
        this.Click(filterButton.Selector);
        return this.Type(_usersTableCollection.GetSearchInput().Selector, filterValue);
    }
    
    public DashboardPage SearchUsersTable(string searchTerm)
    {
        return this.Type(e => e.UsersTableSearchInput, searchTerm);
    }
    
    public DashboardPage SelectUserRow(int rowIndex)
    {
        var row = _usersTableCollection.GetRow(rowIndex);
        return this.Click(row.Selector);
    }
    
    public DashboardPage EditUser(int rowIndex)
    {
        var editButton = _usersTableCollection.GetEditButton(rowIndex);
        return this.Click(editButton.Selector);
    }
    
    public DashboardPage DeleteUser(int rowIndex)
    {
        var deleteButton = _usersTableCollection.GetDeleteButton(rowIndex);
        return this.Click(deleteButton.Selector);
    }
    
    public DashboardPage NavigateToNextPage()
    {
        var nextButton = _usersTableCollection.GetPaginationButton("next");
        return this.Click(nextButton.Selector);
    }
    
    public DashboardPage NavigateToPreviousPage()
    {
        var prevButton = _usersTableCollection.GetPaginationButton("prev");
        return this.Click(prevButton.Selector);
    }
    
    public DashboardPage NavigateToPage(int pageNumber)
    {
        var pageButton = _usersTableCollection.GetPaginationButton($"page-{pageNumber}");
        return this.Click(pageButton.Selector);
    }
    
    // Verification methods for dashboard
    public DashboardPage VerifyDashboardIsLoaded()
    {
        return this.Verify(e => e.DashboardTitle).IsVisible().HasText("User Dashboard");
    }
    
    public DashboardPage VerifyUserStatsAreDisplayed()
    {
        return this.Verify(e => e.UserStats).IsVisible();
    }
    
    public DashboardPage VerifyStatusMessage(string expectedMessage)
    {
        return this.Verify(e => e.StatusMessage).HasText(expectedMessage);
    }
    
    public DashboardPage VerifyUserRowExists(int rowIndex)
    {
        if (!_usersTableCollection.RowExists(rowIndex))
        {
            throw new ElementValidationException($"User row {rowIndex} does not exist");
        }
        return this;
    }
    
    public DashboardPage VerifyUserCellContains(int rowIndex, string columnName, string expectedText)
    {
        if (!_usersTableCollection.CellContainsText(rowIndex, columnName, expectedText))
        {
            var actualText = _usersTableCollection.GetCellText(rowIndex, columnName);
            throw new ElementValidationException($"Expected cell {rowIndex}-{columnName} to contain '{expectedText}', but got '{actualText}'");
        }
        return this;
    }
    
    public DashboardPage VerifyTableRowCount(int expectedCount)
    {
        var actualCount = _usersTableCollection.GetRowCount();
        if (actualCount != expectedCount)
        {
            throw new ElementValidationException($"Expected table to have {expectedCount} rows, but got {actualCount}");
        }
        return this;
    }
    
    public DashboardPage VerifyUserIsSorted(string columnName, string direction = "asc")
    {
        return this.Verify(e => e.UsersTable).HasAttribute("data-sorted-by", columnName);
    }
    
    public DashboardPage VerifyUserIsFiltered(string columnName, string filterValue)
    {
        return this.Verify(e => e.UsersTable).HasAttribute("data-filtered-by", $"{columnName}:{filterValue}");
    }
}
```

### 2. User Edit Modal Implementation

Create a complete UserEditModal class:

```csharp
public class UserEditModal : BasePageComponent<PlaywrightDriver, UserEditModal>
{
    public IElement UserNameInput { get; private set; }
    public IElement UserEmailInput { get; private set; }
    public IElement UserRoleSelect { get; private set; }
    public IElement UserStatusSelect { get; private set; }
    public IElement UserPasswordInput { get; private set; }
    public IElement UserConfirmPasswordInput { get; private set; }
    public IElement SaveButton { get; private set; }
    public IElement CancelButton { get; private set; }
    public IElement CloseButton { get; private set; }
    public IElement ModalTitle { get; private set; }
    public IElement ValidationErrors { get; private set; }
    public IElement ModalOverlay { get; private set; }
    
    public UserEditModal(IServiceProvider serviceProvider, Uri urlPattern) 
        : base(serviceProvider, urlPattern)
    {
    }
    
    protected override void ConfigureElements()
    {
        UserNameInput = new Element("#user-name-input", "User Name Input", ElementType.Input);
        UserEmailInput = new Element("#user-email-input", "User Email Input", ElementType.Input);
        UserRoleSelect = new Element("#user-role-select", "User Role Select", ElementType.Select);
        UserStatusSelect = new Element("#user-status-select", "User Status Select", ElementType.Select);
        UserPasswordInput = new Element("#user-password-input", "User Password Input", ElementType.Input);
        UserConfirmPasswordInput = new Element("#user-confirm-password-input", "User Confirm Password Input", ElementType.Input);
        SaveButton = new Element("#save-button", "Save Button", ElementType.Button);
        CancelButton = new Element("#cancel-button", "Cancel Button", ElementType.Button);
        CloseButton = new Element("#close-button", "Close Button", ElementType.Button);
        ModalTitle = new Element("#modal-title", "Modal Title", ElementType.H1);
        ValidationErrors = new Element("#validation-errors", "Validation Errors", ElementType.Div);
        ModalOverlay = new Element("#modal-overlay", "Modal Overlay", ElementType.Div);
    }
    
    // Modal interaction methods
    public UserEditModal SetUserName(string userName)
    {
        return this.Type(e => e.UserNameInput, userName);
    }
    
    public UserEditModal SetUserEmail(string userEmail)
    {
        return this.Type(e => e.UserEmailInput, userEmail);
    }
    
    public UserEditModal SetUserRole(string userRole)
    {
        return this.Select(e => e.UserRoleSelect, userRole);
    }
    
    public UserEditModal SetUserStatus(string userStatus)
    {
        return this.Select(e => e.UserStatusSelect, userStatus);
    }
    
    public UserEditModal SetUserPassword(string password)
    {
        return this.Type(e => e.UserPasswordInput, password);
    }
    
    public UserEditModal SetUserConfirmPassword(string confirmPassword)
    {
        return this.Type(e => e.UserConfirmPasswordInput, confirmPassword);
    }
    
    public UserEditModal SaveChanges()
    {
        return this.Click(e => e.SaveButton);
    }
    
    public UserEditModal CancelChanges()
    {
        return this.Click(e => e.CancelButton);
    }
    
    public UserEditModal CloseModal()
    {
        return this.Click(e => e.CloseButton);
    }
    
    public UserEditModal ClickOutsideModal()
    {
        return this.Click(e => e.ModalOverlay);
    }
    
    // Verification methods for modal
    public UserEditModal VerifyModalIsVisible()
    {
        return this.Verify(e => e.ModalTitle).IsVisible();
    }
    
    public UserEditModal VerifyModalIsHidden()
    {
        return this.Verify(e => e.ModalOverlay).IsVisible().HasClass("hidden");
    }
    
    public UserEditModal VerifyUserNameIs(string expectedName)
    {
        return this.Verify(e => e.UserNameInput).HasValue(expectedName);
    }
    
    public UserEditModal VerifyUserEmailIs(string expectedEmail)
    {
        return this.Verify(e => e.UserEmailInput).HasValue(expectedEmail);
    }
    
    public UserEditModal VerifyUserRoleIs(string expectedRole)
    {
        return this.Verify(e => e.UserRoleSelect).HasValue(expectedRole);
    }
    
    public UserEditModal VerifyUserStatusIs(string expectedStatus)
    {
        return this.Verify(e => e.UserStatusSelect).HasValue(expectedStatus);
    }
    
    public UserEditModal VerifyValidationErrorsAreDisplayed()
    {
        return this.Verify(e => e.ValidationErrors).IsVisible();
    }
    
    public UserEditModal VerifyValidationErrorContains(string expectedError)
    {
        return this.Verify(e => e.ValidationErrors).ContainsText(expectedError);
    }
}
```

### 3. Sample App HTML Implementation

Create the HTML pages for the sample app:

#### Dashboard Page (HTML)
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Dashboard - FluentUIScaffold Sample</title>
    <style>
        .dashboard {
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
        }
        .dashboard-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
        }
        .user-stats {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }
        .stat-card {
            background: #f8f9fa;
            padding: 20px;
            border-radius: 8px;
            text-align: center;
        }
        .stat-number {
            font-size: 2em;
            font-weight: bold;
            color: #007bff;
        }
        .table-container {
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            overflow: hidden;
        }
        .table-header {
            padding: 20px;
            border-bottom: 1px solid #eee;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        .table-controls {
            display: flex;
            gap: 10px;
            align-items: center;
        }
        .search-input {
            padding: 8px 12px;
            border: 1px solid #ddd;
            border-radius: 4px;
            width: 200px;
        }
        .btn {
            padding: 8px 16px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
        }
        .btn-primary {
            background-color: #007bff;
            color: white;
        }
        .btn-secondary {
            background-color: #6c757d;
            color: white;
        }
        .btn-success {
            background-color: #28a745;
            color: white;
        }
        .btn-danger {
            background-color: #dc3545;
            color: white;
        }
        .users-table {
            width: 100%;
            border-collapse: collapse;
        }
        .users-table th {
            background-color: #f8f9fa;
            padding: 12px;
            text-align: left;
            border-bottom: 2px solid #dee2e6;
        }
        .users-table td {
            padding: 12px;
            border-bottom: 1px solid #dee2e6;
        }
        .users-table tr:hover {
            background-color: #f8f9fa;
        }
        .sort-button, .filter-button {
            background: none;
            border: none;
            cursor: pointer;
            padding: 4px;
        }
        .sort-button:hover, .filter-button:hover {
            background-color: #e9ecef;
            border-radius: 4px;
        }
        .action-buttons {
            display: flex;
            gap: 5px;
        }
        .pagination {
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
            gap: 10px;
        }
        .pagination button {
            padding: 8px 12px;
            border: 1px solid #dee2e6;
            background: white;
            cursor: pointer;
        }
        .pagination button.active {
            background-color: #007bff;
            color: white;
            border-color: #007bff;
        }
        .status-message {
            padding: 10px;
            margin: 10px 0;
            border-radius: 4px;
            display: none;
        }
        .status-message.success {
            background-color: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
        }
        .status-message.error {
            background-color: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
        }
    </style>
</head>
<body>
    <div class="dashboard">
        <div class="dashboard-header">
            <h1 id="dashboard-title">User Dashboard</h1>
            <div class="header-actions">
                <button id="refresh-button" class="btn btn-secondary">Refresh</button>
                <button id="export-button" class="btn btn-primary">Export</button>
                <button id="import-button" class="btn btn-primary">Import</button>
            </div>
        </div>
        
        <div id="user-stats" class="user-stats">
            <div class="stat-card">
                <div class="stat-number" id="total-users">25</div>
                <div class="stat-label">Total Users</div>
            </div>
            <div class="stat-card">
                <div class="stat-number" id="active-users">20</div>
                <div class="stat-label">Active Users</div>
            </div>
            <div class="stat-card">
                <div class="stat-number" id="admin-users">5</div>
                <div class="stat-label">Administrators</div>
            </div>
            <div class="stat-card">
                <div class="stat-number" id="new-users">3</div>
                <div class="stat-label">New This Month</div>
            </div>
        </div>
        
        <div class="table-container">
            <div class="table-header">
                <h2>Users</h2>
                <div class="table-controls">
                    <input type="text" id="users-table-search" class="search-input" placeholder="Search users...">
                    <button id="add-user-button" class="btn btn-success">Add User</button>
                </div>
            </div>
            
            <div id="status-message" class="status-message"></div>
            
            <table id="users-table" class="users-table">
                <thead>
                    <tr>
                        <th>
                            ID
                            <button class="sort-button" data-column="id">↕</button>
                            <button class="filter-button" data-column="id">🔍</button>
                        </th>
                        <th>
                            Name
                            <button class="sort-button" data-column="name">↕</button>
                            <button class="filter-button" data-column="name">🔍</button>
                        </th>
                        <th>
                            Email
                            <button class="sort-button" data-column="email">↕</button>
                            <button class="filter-button" data-column="email">🔍</button>
                        </th>
                        <th>
                            Role
                            <button class="sort-button" data-column="role">↕</button>
                            <button class="filter-button" data-column="role">🔍</button>
                        </th>
                        <th>
                            Status
                            <button class="sort-button" data-column="status">↕</button>
                            <button class="filter-button" data-column="status">🔍</button>
                        </th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody id="users-table-body">
                    <!-- Table rows will be dynamically populated -->
                </tbody>
            </table>
            
            <div class="pagination">
                <button class="prev-button">Previous</button>
                <button class="page-button active" data-page="1">1</button>
                <button class="page-button" data-page="2">2</button>
                <button class="page-button" data-page="3">3</button>
                <button class="next-button">Next</button>
            </div>
        </div>
    </div>

    <script>
        // Sample user data
        let users = [
            { id: 1, name: "John Doe", email: "john@example.com", role: "Administrator", status: "Active" },
            { id: 2, name: "Jane Smith", email: "jane@example.com", role: "User", status: "Active" },
            { id: 3, name: "Bob Johnson", email: "bob@example.com", role: "User", status: "Inactive" },
            { id: 4, name: "Alice Brown", email: "alice@example.com", role: "Administrator", status: "Active" },
            { id: 5, name: "Charlie Wilson", email: "charlie@example.com", role: "User", status: "Active" }
        ];
        
        let currentPage = 1;
        let itemsPerPage = 5;
        let sortColumn = null;
        let sortDirection = 'asc';
        let filterColumn = null;
        let filterValue = '';
        
        function renderUsers() {
            const tbody = document.getElementById('users-table-body');
            tbody.innerHTML = '';
            
            let filteredUsers = users;
            
            // Apply filter
            if (filterColumn && filterValue) {
                filteredUsers = users.filter(user => 
                    user[filterColumn].toLowerCase().includes(filterValue.toLowerCase())
                );
            }
            
            // Apply sort
            if (sortColumn) {
                filteredUsers.sort((a, b) => {
                    let aVal = a[sortColumn];
                    let bVal = b[sortColumn];
                    
                    if (sortDirection === 'asc') {
                        return aVal.localeCompare(bVal);
                    } else {
                        return bVal.localeCompare(aVal);
                    }
                });
            }
            
            // Apply pagination
            const startIndex = (currentPage - 1) * itemsPerPage;
            const endIndex = startIndex + itemsPerPage;
            const pageUsers = filteredUsers.slice(startIndex, endIndex);
            
            pageUsers.forEach(user => {
                const row = document.createElement('tr');
                row.className = 'user-row';
                row.dataset.userId = user.id;
                
                row.innerHTML = `
                    <td>${user.id}</td>
                    <td>${user.name}</td>
                    <td>${user.email}</td>
                    <td>${user.role}</td>
                    <td>${user.status}</td>
                    <td class="action-buttons">
                        <button class="edit-button btn btn-primary" data-user-id="${user.id}">Edit</button>
                        <button class="delete-button btn btn-danger" data-user-id="${user.id}">Delete</button>
                    </td>
                `;
                
                tbody.appendChild(row);
            });
            
            updatePagination(filteredUsers.length);
        }
        
        function updatePagination(totalItems) {
            const totalPages = Math.ceil(totalItems / itemsPerPage);
            const pagination = document.querySelector('.pagination');
            
            // Update page buttons
            const pageButtons = pagination.querySelectorAll('.page-button');
            pageButtons.forEach((button, index) => {
                const pageNum = index + 1;
                button.textContent = pageNum;
                button.classList.toggle('active', pageNum === currentPage);
                button.style.display = pageNum <= totalPages ? 'block' : 'none';
            });
        }
        
        function showStatusMessage(message, type = 'success') {
            const statusElement = document.getElementById('status-message');
            statusElement.textContent = message;
            statusElement.className = `status-message ${type}`;
            statusElement.style.display = 'block';
            
            setTimeout(() => {
                statusElement.style.display = 'none';
            }, 3000);
        }
        
        // Event listeners
        document.addEventListener('DOMContentLoaded', () => {
            renderUsers();
            
            // Sort buttons
            document.querySelectorAll('.sort-button').forEach(button => {
                button.addEventListener('click', () => {
                    const column = button.dataset.column;
                    
                    if (sortColumn === column) {
                        sortDirection = sortDirection === 'asc' ? 'desc' : 'asc';
                    } else {
                        sortColumn = column;
                        sortDirection = 'asc';
                    }
                    
                    currentPage = 1;
                    renderUsers();
                });
            });
            
            // Search input
            document.getElementById('users-table-search').addEventListener('input', (e) => {
                filterValue = e.target.value;
                currentPage = 1;
                renderUsers();
            });
            
            // Pagination
            document.querySelector('.prev-button').addEventListener('click', () => {
                if (currentPage > 1) {
                    currentPage--;
                    renderUsers();
                }
            });
            
            document.querySelector('.next-button').addEventListener('click', () => {
                const totalPages = Math.ceil(users.length / itemsPerPage);
                if (currentPage < totalPages) {
                    currentPage++;
                    renderUsers();
                }
            });
            
            // Page buttons
            document.querySelectorAll('.page-button').forEach(button => {
                button.addEventListener('click', () => {
                    currentPage = parseInt(button.dataset.page);
                    renderUsers();
                });
            });
            
            // Edit buttons
            document.addEventListener('click', (e) => {
                if (e.target.classList.contains('edit-button')) {
                    const userId = e.target.dataset.userId;
                    const user = users.find(u => u.id == userId);
                    if (user) {
                        // Open edit modal
                        openEditModal(user);
                    }
                }
            });
            
            // Delete buttons
            document.addEventListener('click', (e) => {
                if (e.target.classList.contains('delete-button')) {
                    const userId = e.target.dataset.userId;
                    if (confirm('Are you sure you want to delete this user?')) {
                        users = users.filter(u => u.id != userId);
                        renderUsers();
                        showStatusMessage('User deleted successfully');
                    }
                }
            });
            
            // Add user button
            document.getElementById('add-user-button').addEventListener('click', () => {
                openEditModal(null);
            });
            
            // Refresh button
            document.getElementById('refresh-button').addEventListener('click', () => {
                renderUsers();
                showStatusMessage('Data refreshed successfully');
            });
        });
        
        function openEditModal(user) {
            // This would open a modal for editing users
            // For now, just show a message
            if (user) {
                showStatusMessage(`Editing user: ${user.name}`);
            } else {
                showStatusMessage('Adding new user');
            }
        }
    </script>
</body>
</html>
```

### 4. Working Example

Create a working example that demonstrates dashboard functionality:

```csharp
[TestMethod]
public async Task Can_Interact_With_Dashboard_Data_Table()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web(options =>
    {
        options.BaseUrl = new Uri("https://your-app.com");
    });
    
    // Act - Navigate to dashboard and interact with data table
    var dashboard = fluentUI.NavigateTo<DashboardPage>();
    
    dashboard
        .VerifyDashboardIsLoaded()
        .VerifyUserStatsAreDisplayed()
        .SortUsersTable("name")
        .SearchUsersTable("admin")
        .SelectUserRow(2)
        .EditUser(2);
    
    // Act - Edit user in modal
    var userEditModal = fluentUI.NavigateTo<UserEditModal>();
    
    userEditModal
        .VerifyModalIsVisible()
        .SetUserName("newadmin")
        .SetUserEmail("newadmin@example.com")
        .SetUserRole("Administrator")
        .SetUserStatus("Active")
        .SaveChanges();
    
    // Assert - Verify changes
    dashboard
        .VerifyStatusMessage("User updated successfully")
        .VerifyUserCellContains(2, "name", "newadmin")
        .VerifyUserCellContains(2, "email", "newadmin@example.com")
        .VerifyUserCellContains(2, "role", "Administrator");
}
```

## Implementation Tasks

### Phase 1: Dashboard Page Implementation
1. [ ] Create DashboardPage with all elements
2. [ ] Implement data table interaction methods
3. [ ] Add verification methods for dashboard
4. [ ] Test all dashboard functionality

### Phase 2: User Edit Modal
1. [ ] Create UserEditModal with all elements
2. [ ] Implement modal interaction methods
3. [ ] Add verification methods for modal
4. [ ] Test modal functionality

### Phase 3: Sample App HTML
1. [ ] Create dashboard page HTML with styling
2. [ ] Add JavaScript for data table functionality
3. [ ] Implement sorting, filtering, and pagination
4. [ ] Add user management features

### Phase 4: Integration and Testing
1. [ ] Add pages to sample app routing
2. [ ] Test all dashboard functionality
3. [ ] Test data table interactions
4. [ ] Create comprehensive tests

## Dependencies

- **Story 3.1.1**: Implement Data Table Interactions (must be completed first)

## Estimation

- **Time Estimate**: 3-4 weeks
- **Complexity**: Medium
- **Risk**: Medium (complex data table interactions)

## Definition of Done

- [ ] DashboardPage class is implemented and working
- [ ] UserEditModal class is implemented and working
- [ ] Sample app has working dashboard page
- [ ] Data table functionality works correctly
- [ ] Sorting, filtering, and pagination work correctly
- [ ] Comprehensive tests are passing
- [ ] Working example demonstrates dashboard functionality
- [ ] All acceptance criteria are met

## Notes

- The sample app should provide realistic dashboard functionality
- Data table interactions should be smooth and responsive
- Modal interactions should be robust
- All dashboard features should work with the fluent API 