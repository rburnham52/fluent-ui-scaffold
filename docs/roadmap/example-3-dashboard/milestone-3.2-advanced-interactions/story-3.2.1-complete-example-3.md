# Story 3.2.1: Complete Example 3 Implementation

## Overview

Complete the implementation of Example 3 (Dashboard with Data Tables) by integrating all the components from previous stories and creating a comprehensive working example that demonstrates the complete dashboard capabilities.

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

This story focuses on completing the implementation and creating comprehensive tests.

## Acceptance Criteria

- [ ] Implement complete Example 3 scenario
- [ ] Add comprehensive tests for dashboard functionality
- [ ] Update documentation with Example 3
- [ ] All tests pass and demonstrate working framework

## Technical Requirements

### 1. Complete Example 3 Implementation

Create a complete working example that demonstrates the full dashboard flow:

```csharp
[TestMethod]
public async Task Can_Complete_Dashboard_Data_Table_Flow()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web(options =>
    {
        options.BaseUrl = new Uri("https://your-app.com");
        options.DefaultTimeout = TimeSpan.FromSeconds(30);
        options.LogLevel = LogLevel.Information;
    });
    
    try
    {
        // Act - Navigate to dashboard and interact with data table
        var dashboard = fluentUI.NavigateTo<DashboardPage>();
        
        dashboard
            .VerifyDashboardIsLoaded()
            .VerifyUserStatsAreDisplayed()
            .Click(e => e.UsersTableSortButton)
            .Click(e => e.UsersTableFilterButton)
            .Type(e => e.UsersTableSearchInput, "admin")
            .Click(e => e.UsersTableRow(2))
            .Click(e => e.UsersTableEditButton);
        
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
            .Verify(e => e.StatusMessage).HasText("User updated successfully")
            .VerifyUserCellContains(2, "name", "newadmin")
            .VerifyUserCellContains(2, "email", "newadmin@example.com")
            .VerifyUserCellContains(2, "role", "Administrator");
    }
    finally
    {
        fluentUI?.Dispose();
    }
}
```

### 2. Comprehensive Test Suite

Create a comprehensive test suite that covers all aspects of Example 3:

#### Dashboard Tests
```csharp
[TestClass]
public class DashboardTests
{
    private FluentUIScaffoldApp<WebApp> _fluentUI;
    
    [TestInitialize]
    public void Setup()
    {
        _fluentUI = FluentUIScaffoldBuilder.Web(options =>
        {
            options.BaseUrl = TestConfiguration.BaseUri;
            options.DefaultTimeout = TimeSpan.FromSeconds(30);
        });
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _fluentUI?.Dispose();
    }
    
    [TestMethod]
    public async Task Can_Load_Dashboard()
    {
        // Arrange
        var dashboard = _fluentUI.NavigateTo<DashboardPage>();
        
        // Assert
        dashboard
            .VerifyDashboardIsLoaded()
            .VerifyUserStatsAreDisplayed()
            .VerifyTableRowCount(5);
    }
    
    [TestMethod]
    public async Task Can_Sort_Users_Table()
    {
        // Arrange
        var dashboard = _fluentUI.NavigateTo<DashboardPage>();
        
        // Act
        dashboard.SortUsersTable("name");
        
        // Assert
        dashboard.VerifyUserIsSorted("name", "asc");
    }
    
    [TestMethod]
    public async Task Can_Filter_Users_Table()
    {
        // Arrange
        var dashboard = _fluentUI.NavigateTo<DashboardPage>();
        
        // Act
        dashboard.SearchUsersTable("admin");
        
        // Assert
        dashboard.VerifyUserIsFiltered("name", "admin");
    }
    
    [TestMethod]
    public async Task Can_Select_User_Row()
    {
        // Arrange
        var dashboard = _fluentUI.NavigateTo<DashboardPage>();
        
        // Act
        dashboard.SelectUserRow(2);
        
        // Assert
        dashboard.VerifyUserRowExists(2);
    }
    
    [TestMethod]
    public async Task Can_Navigate_Table_Pages()
    {
        // Arrange
        var dashboard = _fluentUI.NavigateTo<DashboardPage>();
        
        // Act
        dashboard.NavigateToNextPage();
        
        // Assert
        dashboard.Verify(e => e.PaginationInfo).ContainsText("Page 2");
    }
}
```

#### User Edit Modal Tests
```csharp
[TestClass]
public class UserEditModalTests
{
    private FluentUIScaffoldApp<WebApp> _fluentUI;
    
    [TestInitialize]
    public void Setup()
    {
        _fluentUI = FluentUIScaffoldBuilder.Web(options =>
        {
            options.BaseUrl = TestConfiguration.BaseUri;
            options.DefaultTimeout = TimeSpan.FromSeconds(30);
        });
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _fluentUI?.Dispose();
    }
    
    [TestMethod]
    public async Task Can_Edit_User()
    {
        // Arrange
        var dashboard = _fluentUI.NavigateTo<DashboardPage>();
        dashboard.EditUser(2);
        
        var userEditModal = _fluentUI.NavigateTo<UserEditModal>();
        
        // Act
        userEditModal
            .SetUserName("updateduser")
            .SetUserEmail("updated@example.com")
            .SetUserRole("User")
            .SetUserStatus("Active")
            .SaveChanges();
        
        // Assert
        dashboard.Verify(e => e.StatusMessage).HasText("User updated successfully");
    }
    
    [TestMethod]
    public async Task Can_Cancel_User_Edit()
    {
        // Arrange
        var dashboard = _fluentUI.NavigateTo<DashboardPage>();
        dashboard.EditUser(2);
        
        var userEditModal = _fluentUI.NavigateTo<UserEditModal>();
        
        // Act
        userEditModal.CancelChanges();
        
        // Assert
        userEditModal.VerifyModalIsHidden();
    }
    
    [TestMethod]
    public async Task Can_Validate_User_Form()
    {
        // Arrange
        var dashboard = _fluentUI.NavigateTo<DashboardPage>();
        dashboard.EditUser(2);
        
        var userEditModal = _fluentUI.NavigateTo<UserEditModal>();
        
        // Act
        userEditModal
            .SetUserName("")
            .SetUserEmail("invalid-email")
            .SaveChanges();
        
        // Assert
        userEditModal
            .VerifyValidationErrorsAreDisplayed()
            .VerifyValidationErrorContains("Name is required")
            .VerifyValidationErrorContains("Invalid email format");
    }
}
```

#### Integration Tests
```csharp
[TestClass]
public class DashboardIntegrationTests
{
    private FluentUIScaffoldApp<WebApp> _fluentUI;
    
    [TestInitialize]
    public void Setup()
    {
        _fluentUI = FluentUIScaffoldBuilder.Web(options =>
        {
            options.BaseUrl = TestConfiguration.BaseUri;
            options.DefaultTimeout = TimeSpan.FromSeconds(30);
        });
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _fluentUI?.Dispose();
    }
    
    [TestMethod]
    public async Task Can_Complete_Full_Dashboard_Flow()
    {
        // Arrange
        var dashboard = _fluentUI.NavigateTo<DashboardPage>();
        
        // Act - Sort and filter users
        dashboard
            .SortUsersTable("name")
            .SearchUsersTable("admin")
            .SelectUserRow(2);
        
        // Act - Edit user
        dashboard.EditUser(2);
        
        var userEditModal = _fluentUI.NavigateTo<UserEditModal>();
        
        userEditModal
            .SetUserName("newadmin")
            .SetUserEmail("newadmin@example.com")
            .SetUserRole("Administrator")
            .SetUserStatus("Active")
            .SaveChanges();
        
        // Assert - Verify all changes
        dashboard
            .Verify(e => e.StatusMessage).HasText("User updated successfully")
            .VerifyUserCellContains(2, "name", "newadmin")
            .VerifyUserCellContains(2, "email", "newadmin@example.com")
            .VerifyUserCellContains(2, "role", "Administrator")
            .VerifyUserCellContains(2, "status", "Active");
    }
    
    [TestMethod]
    public async Task Can_Handle_Complex_Table_Operations()
    {
        // Arrange
        var dashboard = _fluentUI.NavigateTo<DashboardPage>();
        
        // Act - Perform complex table operations
        dashboard
            .SortUsersTable("role")
            .SearchUsersTable("user")
            .NavigateToNextPage()
            .SelectUserRow(1)
            .EditUser(1);
        
        var userEditModal = _fluentUI.NavigateTo<UserEditModal>();
        
        userEditModal
            .SetUserStatus("Inactive")
            .SaveChanges();
        
        // Assert
        dashboard
            .Verify(e => e.StatusMessage).HasText("User updated successfully")
            .VerifyUserCellContains(1, "status", "Inactive");
    }
    
    [TestMethod]
    public async Task Can_Handle_Table_Pagination()
    {
        // Arrange
        var dashboard = _fluentUI.NavigateTo<DashboardPage>();
        
        // Act - Navigate through pages
        dashboard
            .NavigateToNextPage()
            .NavigateToNextPage()
            .NavigateToPreviousPage();
        
        // Assert
        dashboard.Verify(e => e.PaginationInfo).ContainsText("Page 2");
    }
    
    [TestMethod]
    public async Task Can_Handle_Table_Sorting_And_Filtering()
    {
        // Arrange
        var dashboard = _fluentUI.NavigateTo<DashboardPage>();
        
        // Act - Sort and filter
        dashboard
            .SortUsersTable("email")
            .SearchUsersTable("example.com");
        
        // Assert
        dashboard
            .VerifyUserIsSorted("email", "asc")
            .VerifyUserIsFiltered("email", "example.com");
    }
}
```

### 3. Documentation Updates

Update the documentation to include Example 3:

#### API Documentation
```markdown
# Example 3: Dashboard with Data Tables

This example demonstrates advanced data table interactions and modal operations in FluentUIScaffold V2.0, including:

- Complex data table interactions
- Row selection and editing
- Modal form interactions
- Advanced verification patterns

## Complete Example

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

## Key Features Demonstrated

1. **Data Table Interactions**: Sorting, filtering, and pagination
2. **Row Selection**: Selecting specific rows for editing
3. **Modal Operations**: Form interactions in modal dialogs
4. **Complex Verification**: Multiple verification patterns
5. **Advanced Interactions**: Multi-step workflows with state management
```

### 4. Sample App Integration

Ensure the sample app is fully integrated with Example 3:

#### Navigation Updates
Update the navigation to include dashboard links:

```html
<nav>
    <a href="/">Home</a>
    <a href="/dashboard">Dashboard</a>
    <a href="/catalog">Products</a>
    <a href="/cart">Cart</a>
    <a href="/register">Register</a>
    <a href="/login">Login</a>
</nav>
```

#### Home Page Updates
Update the home page to include dashboard features:

```html
<div class="featured-section">
    <h2>Application Features</h2>
    <div class="dashboard-preview">
        <h3>User Dashboard</h3>
        <p>Manage users with advanced data table interactions</p>
        <a href="/dashboard" class="btn btn-primary">View Dashboard</a>
    </div>
    <div class="shopping-preview">
        <h3>Shopping Cart</h3>
        <p>Browse products and manage your cart</p>
        <a href="/catalog" class="btn btn-secondary">Shop Now</a>
    </div>
</div>
```

## Implementation Tasks

### Phase 1: Complete Implementation
1. [ ] Integrate all components from previous stories
2. [ ] Create complete working example
3. [ ] Test all dashboard functionality
4. [ ] Verify all data table interactions work correctly

### Phase 2: Comprehensive Testing
1. [ ] Create dashboard tests
2. [ ] Create user edit modal tests
3. [ ] Create integration tests
4. [ ] Test error scenarios and validation

### Phase 3: Documentation
1. [ ] Update API documentation with Example 3
2. [ ] Create tutorials and best practices
3. [ ] Add code examples and explanations
4. [ ] Update sample app documentation

### Phase 4: Sample App Integration
1. [ ] Add navigation between pages
2. [ ] Ensure all dashboard functionality works correctly
3. [ ] Test complete dashboard flows
4. [ ] Verify all tests pass

## Dependencies

- **Story 3.1.1**: Implement Data Table Interactions (must be completed first)
- **Story 3.1.2**: Create Dashboard Pages (must be completed first)

## Estimation

- **Time Estimate**: 2-3 weeks
- **Complexity**: Medium
- **Risk**: Low (building on established foundation)

## Definition of Done

- [ ] Complete Example 3 scenario is implemented and working
- [ ] Comprehensive test suite is created and passing
- [ ] Documentation is updated with Example 3
- [ ] Sample app is fully integrated
- [ ] All dashboard functionality works correctly
- [ ] All data table interactions work correctly
- [ ] Modal operations work correctly
- [ ] All acceptance criteria are met

## Notes

- This story represents the completion of Example 3
- All previous stories must be completed before this one
- The implementation should demonstrate advanced framework capabilities
- The test suite should be comprehensive and cover all scenarios
- Documentation should be clear and helpful for developers 