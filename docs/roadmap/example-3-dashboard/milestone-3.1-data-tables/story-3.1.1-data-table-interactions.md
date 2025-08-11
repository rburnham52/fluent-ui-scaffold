# Story 3.1.1: Implement Data Table Interactions

## Overview

Implement data table interactions required for Example 3 (Dashboard with Data Tables), including sorting, filtering, pagination, and row selection capabilities.

## Background

Example 3 from the V2.0 specification shows complex data table interactions:

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

This story focuses on implementing data table interactions and row-based operations.

## Acceptance Criteria

- [ ] Implement data table sorting functionality
- [ ] Implement data table filtering functionality
- [ ] Implement row selection and editing
- [ ] Create working example with data table interactions

## Technical Requirements

### 1. Data Table Element Collection

Create a specialized element collection for data tables:

```csharp
public class DataTableElement : IElement
{
    public string Selector { get; }
    public string Name { get; }
    public ElementType Type { get; }
    public int RowIndex { get; }
    public string ColumnName { get; }
    
    public DataTableElement(string selector, string name, int rowIndex, string columnName = null)
    {
        Selector = selector;
        Name = name;
        Type = ElementType.Div;
        RowIndex = rowIndex;
        ColumnName = columnName;
    }
}

public class DataTableCollection : IElementCollection
{
    private readonly List<DataTableElement> _rows = new();
    private readonly IUIDriver _driver;
    private readonly FluentUIScaffoldOptions _options;
    
    public DataTableCollection(IUIDriver driver, FluentUIScaffoldOptions options, string tableSelector)
    {
        _driver = driver;
        _options = options;
        TableSelector = tableSelector;
    }
    
    public string TableSelector { get; }
    
    public DataTableElement GetRow(int rowIndex)
    {
        var selector = $"{TableSelector} tbody tr:nth-child({rowIndex + 1})";
        return new DataTableElement(selector, $"Table Row {rowIndex}", rowIndex);
    }
    
    public DataTableElement GetCell(int rowIndex, string columnName)
    {
        var columnIndex = GetColumnIndex(columnName);
        var selector = $"{TableSelector} tbody tr:nth-child({rowIndex + 1}) td:nth-child({columnIndex + 1})";
        return new DataTableElement(selector, $"Table Cell {rowIndex}-{columnName}", rowIndex, columnName);
    }
    
    public DataTableElement GetSortButton(string columnName)
    {
        var columnIndex = GetColumnIndex(columnName);
        var selector = $"{TableSelector} thead tr th:nth-child({columnIndex + 1}) .sort-button";
        return new DataTableElement(selector, $"Sort Button {columnName}", -1, columnName);
    }
    
    public DataTableElement GetFilterButton(string columnName)
    {
        var columnIndex = GetColumnIndex(columnName);
        var selector = $"{TableSelector} thead tr th:nth-child({columnIndex + 1}) .filter-button";
        return new DataTableElement(selector, $"Filter Button {columnName}", -1, columnName);
    }
    
    public DataTableElement GetSearchInput()
    {
        var selector = $"{TableSelector} .search-input";
        return new DataTableElement(selector, "Table Search Input", -1);
    }
    
    public DataTableElement GetPaginationButton(string action)
    {
        var selector = $"{TableSelector} .pagination .{action}-button";
        return new DataTableElement(selector, $"Pagination {action} Button", -1);
    }
    
    public DataTableElement GetEditButton(int rowIndex)
    {
        var selector = $"{TableSelector} tbody tr:nth-child({rowIndex + 1}) .edit-button";
        return new DataTableElement(selector, $"Edit Button Row {rowIndex}", rowIndex);
    }
    
    public DataTableElement GetDeleteButton(int rowIndex)
    {
        var selector = $"{TableSelector} tbody tr:nth-child({rowIndex + 1}) .delete-button";
        return new DataTableElement(selector, $"Delete Button Row {rowIndex}", rowIndex);
    }
    
    private int GetColumnIndex(string columnName)
    {
        // This would be determined by the actual table structure
        // For now, return a default mapping
        return columnName?.ToLower() switch
        {
            "id" => 0,
            "name" => 1,
            "email" => 2,
            "role" => 3,
            "status" => 4,
            "actions" => 5,
            _ => 0
        };
    }
    
    public int GetRowCount()
    {
        var rowSelector = $"{TableSelector} tbody tr";
        return _driver.GetElementCount(rowSelector);
    }
    
    public bool RowExists(int rowIndex)
    {
        return rowIndex >= 0 && rowIndex < GetRowCount();
    }
    
    public string GetCellText(int rowIndex, string columnName)
    {
        var cell = GetCell(rowIndex, columnName);
        return _driver.GetText(cell.Selector);
    }
    
    public bool CellContainsText(int rowIndex, string columnName, string expectedText)
    {
        var cellText = GetCellText(rowIndex, columnName);
        return cellText.Contains(expectedText);
    }
}
```

### 2. Dashboard Page Implementation

Create the DashboardPage with data table interactions:

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
        
        _usersTableCollection = new DataTableCollection(Driver, Options, "#users-table");
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
    
    // Verification methods for data table
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
        // Implementation would check if the table is sorted by the specified column
        return this.Verify(e => e.UsersTable).HasAttribute("data-sorted-by", columnName);
    }
    
    public DashboardPage VerifyUserIsFiltered(string columnName, string filterValue)
    {
        // Implementation would check if the table is filtered by the specified column
        return this.Verify(e => e.UsersTable).HasAttribute("data-filtered-by", $"{columnName}:{filterValue}");
    }
}
```

### 3. User Edit Modal Implementation

Create a modal for editing users:

```csharp
public class UserEditModal : BasePageComponent<PlaywrightDriver, UserEditModal>
{
    public IElement UserNameInput { get; private set; }
    public IElement UserEmailInput { get; private set; }
    public IElement UserRoleSelect { get; private set; }
    public IElement UserStatusSelect { get; private set; }
    public IElement SaveButton { get; private set; }
    public IElement CancelButton { get; private set; }
    public IElement CloseButton { get; private set; }
    public IElement ModalTitle { get; private set; }
    
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
        SaveButton = new Element("#save-button", "Save Button", ElementType.Button);
        CancelButton = new Element("#cancel-button", "Cancel Button", ElementType.Button);
        CloseButton = new Element("#close-button", "Close Button", ElementType.Button);
        ModalTitle = new Element("#modal-title", "Modal Title", ElementType.H1);
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
    
    // Verification methods for modal
    public UserEditModal VerifyModalIsVisible()
    {
        return this.Verify(e => e.ModalTitle).IsVisible();
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
}
```

### 4. Working Example

Create a working example that demonstrates data table interactions:

```csharp
[TestMethod]
public async Task Can_Interact_With_Data_Table()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web(options =>
    {
        options.BaseUrl = new Uri("https://your-app.com");
    });
    
    // Act - Navigate to dashboard and interact with data table
    var dashboard = fluentUI.NavigateTo<DashboardPage>();
    
    dashboard
        .Click(e => e.UsersTableSortButton)
        .Click(e => e.UsersTableFilterButton)
        .Type(e => e.UsersTableSearchInput, "admin")
        .SelectUserRow(2)
        .EditUser(2);
    
    // Act - Edit user in modal
    var userEditModal = fluentUI.NavigateTo<UserEditModal>();
    
    userEditModal
        .SetUserName("newadmin")
        .SetUserEmail("newadmin@example.com")
        .SetUserRole("Administrator")
        .SetUserStatus("Active")
        .SaveChanges();
    
    // Assert - Verify changes
    dashboard
        .Verify(e => e.StatusMessage).HasText("User updated successfully")
        .VerifyUserCellContains(2, "name", "newadmin")
        .VerifyUserCellContains(2, "email", "newadmin@example.com")
        .VerifyUserCellContains(2, "role", "Administrator");
}
```

## Implementation Tasks

### Phase 1: Data Table Collection
1. [ ] Create `DataTableElement` class
2. [ ] Create `DataTableCollection` class
3. [ ] Implement row and cell selection methods
4. [ ] Add sorting and filtering capabilities

### Phase 2: Dashboard Page
1. [ ] Create DashboardPage with data table elements
2. [ ] Implement data table interaction methods
3. [ ] Add verification methods for data table
4. [ ] Test all data table operations

### Phase 3: User Edit Modal
1. [ ] Create UserEditModal class
2. [ ] Implement modal interaction methods
3. [ ] Add verification methods for modal
4. [ ] Test modal functionality

### Phase 4: Integration and Testing
1. [ ] Create comprehensive tests for data table interactions
2. [ ] Test sorting, filtering, and pagination
3. [ ] Test row selection and editing
4. [ ] Create working examples

## Dependencies

- **Story 2.2.2**: Complete Example 2 Implementation (must be completed first)

## Estimation

- **Time Estimate**: 3-4 weeks
- **Complexity**: Medium
- **Risk**: Medium (complex data table interactions)

## Definition of Done

- [ ] Data table collection is implemented and working
- [ ] Dashboard page with data table interactions is created
- [ ] User edit modal is implemented and working
- [ ] All data table operations work correctly
- [ ] Comprehensive tests are passing
- [ ] Working example demonstrates data table interactions
- [ ] All acceptance criteria are met

## Notes

- Data table interactions should be framework-agnostic
- Row and cell selection should be intuitive
- Sorting and filtering should be efficient
- Modal interactions should be robust
- Error handling should be comprehensive 