# Story 2.2.2: Complete Example 2 Implementation

## Overview

Complete the implementation of Example 2 (Shopping Cart with Dynamic Pricing) by integrating all the components from previous stories and creating a comprehensive working example that demonstrates the complete shopping cart capabilities.

## Background

Example 2 from the V2.0 specification shows a complete shopping cart flow with dynamic pricing:

```csharp
fluentUI
    .NavigateTo<ProductCatalogPage>()
    .Click(e => e.PremiumWidgetAddButton)
    .Click(e => e.PremiumWidgetAddButton)
    .Click(e => e.StandardWidgetAddButton)
    .NavigateTo<ShoppingCartPage>()
    .Verify(e => e.CartItemCount, "3")
    .Verify(e => e.SubTotalElement, "$299.99")
    .Verify(e => e.TaxAmount, "$24.00")
    .Verify(e => e.ShippingCost.HasText("$15.99"))
    .Verify(e => e.TotalAmount.IsVisible().HasValue("$339.98"));
```

This story focuses on completing the implementation and creating comprehensive tests.

## Acceptance Criteria

- [ ] Implement complete Example 2 scenario
- [ ] Add comprehensive tests for shopping cart flow
- [ ] Update documentation with Example 2
- [ ] All tests pass and demonstrate working framework

## Technical Requirements

### 1. Complete Example 2 Implementation

Create a complete working example that demonstrates the full shopping cart flow:

```csharp
[TestMethod]
public async Task Can_Complete_Shopping_Cart_With_Dynamic_Pricing()
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
        // Act - Add items to cart
        var productCatalog = fluentUI.NavigateTo<ProductCatalogPage>();
        
        productCatalog
            .AddPremiumWidget()
            .AddPremiumWidget()
            .AddStandardWidget()
            .VerifyCartItemCount(3);
        
        // Act - View shopping cart
        var shoppingCart = fluentUI.NavigateTo<ShoppingCartPage>();
        
        // Assert - Verify cart calculations
        shoppingCart
            .Verify(e => e.CartItemCount, "3")
            .Verify(e => e.SubTotalElement, "$299.99")
            .Verify(e => e.TaxAmount, "$24.00")
            .Verify(e => e.ShippingCost).HasText("$15.99")
            .Verify(e => e.TotalAmount).IsVisible().HasValue("$339.98")
            .Verify(e => e.CheckoutButton).IsEnabled().IsVisible();
    }
    finally
    {
        fluentUI?.Dispose();
    }
}
```

### 2. Comprehensive Test Suite

Create a comprehensive test suite that covers all aspects of Example 2:

#### Product Catalog Tests
```csharp
[TestClass]
public class ProductCatalogTests
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
    public async Task Can_Add_Items_To_Cart()
    {
        // Arrange
        var productCatalog = _fluentUI.NavigateTo<ProductCatalogPage>();
        
        // Act
        productCatalog
            .AddPremiumWidget()
            .AddStandardWidget()
            .AddBasicWidget();
        
        // Assert
        productCatalog.VerifyCartItemCount(3);
    }
    
    [TestMethod]
    public async Task Can_Search_Products()
    {
        // Arrange
        var productCatalog = _fluentUI.NavigateTo<ProductCatalogPage>();
        
        // Act
        productCatalog.SearchProducts("Premium");
        
        // Assert
        productCatalog.Verify(e => e.ProductList).ContainsText("Premium Widget");
    }
    
    [TestMethod]
    public async Task Can_Filter_Products()
    {
        // Arrange
        var productCatalog = _fluentUI.NavigateTo<ProductCatalogPage>();
        
        // Act
        productCatalog.FilterByCategory("Premium");
        
        // Assert
        productCatalog.Verify(e => e.ProductList).ContainsText("Premium Widget");
    }
}
```

#### Shopping Cart Tests
```csharp
[TestClass]
public class ShoppingCartTests
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
    public async Task Can_View_Cart_With_Items()
    {
        // Arrange
        var productCatalog = _fluentUI.NavigateTo<ProductCatalogPage>();
        productCatalog.AddPremiumWidget().AddStandardWidget();
        
        // Act
        var shoppingCart = _fluentUI.NavigateTo<ShoppingCartPage>();
        
        // Assert
        shoppingCart
            .VerifyItemCount(2)
            .Verify(e => e.SubTotalElement, "$249.98")
            .Verify(e => e.TaxAmount, "$20.00")
            .Verify(e => e.ShippingCost).HasText("$15.99")
            .Verify(e => e.TotalAmount).IsVisible().HasValue("$285.97");
    }
    
    [TestMethod]
    public async Task Can_Remove_Items_From_Cart()
    {
        // Arrange
        var productCatalog = _fluentUI.NavigateTo<ProductCatalogPage>();
        productCatalog.AddPremiumWidget().AddStandardWidget();
        
        var shoppingCart = _fluentUI.NavigateTo<ShoppingCartPage>();
        
        // Act
        shoppingCart.RemoveItem("premium-widget");
        
        // Assert
        shoppingCart.VerifyItemCount(1);
    }
    
    [TestMethod]
    public async Task Can_Update_Item_Quantity()
    {
        // Arrange
        var productCatalog = _fluentUI.NavigateTo<ProductCatalogPage>();
        productCatalog.AddPremiumWidget();
        
        var shoppingCart = _fluentUI.NavigateTo<ShoppingCartPage>();
        
        // Act
        shoppingCart.UpdateQuantity("premium-widget", 3);
        
        // Assert
        shoppingCart.VerifyItemCount(3);
    }
    
    [TestMethod]
    public async Task Can_Clear_Cart()
    {
        // Arrange
        var productCatalog = _fluentUI.NavigateTo<ProductCatalogPage>();
        productCatalog.AddPremiumWidget().AddStandardWidget();
        
        var shoppingCart = _fluentUI.NavigateTo<ShoppingCartPage>();
        
        // Act
        shoppingCart.ClearCart();
        
        // Assert
        shoppingCart.Verify(e => e.EmptyCartMessage).IsVisible().HasText("Your cart is empty");
    }
}
```

#### Integration Tests
```csharp
[TestClass]
public class ShoppingCartIntegrationTests
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
    public async Task Can_Complete_Full_Shopping_Cart_Flow()
    {
        // Arrange
        var testItems = new[]
        {
            new { Name = "Premium Widget", Price = 149.99m, Quantity = 2 },
            new { Name = "Standard Widget", Price = 99.99m, Quantity = 1 },
            new { Name = "Basic Widget", Price = 49.99m, Quantity = 1 }
        };
        
        // Act - Add items to cart
        var productCatalog = _fluentUI.NavigateTo<ProductCatalogPage>();
        
        foreach (var item in testItems)
        {
            for (int i = 0; i < item.Quantity; i++)
            {
                if (item.Name.Contains("Premium"))
                    productCatalog.AddPremiumWidget();
                else if (item.Name.Contains("Standard"))
                    productCatalog.AddStandardWidget();
                else
                    productCatalog.AddBasicWidget();
            }
        }
        
        // Act - View shopping cart
        var shoppingCart = _fluentUI.NavigateTo<ShoppingCartPage>();
        
        // Assert - Verify cart calculations
        var expectedSubtotal = testItems.Sum(item => item.Price * item.Quantity);
        var expectedTax = expectedSubtotal * 0.08m;
        var expectedShipping = 15.99m;
        var expectedTotal = expectedSubtotal + expectedTax + expectedShipping;
        var expectedItemCount = testItems.Sum(item => item.Quantity);
        
        shoppingCart
            .VerifyItemCount(expectedItemCount)
            .Verify(e => e.SubTotalElement, expectedSubtotal.ToString("C"))
            .Verify(e => e.TaxAmount, expectedTax.ToString("C"))
            .Verify(e => e.ShippingCost).HasText(expectedShipping.ToString("C"))
            .Verify(e => e.TotalAmount).IsVisible().HasValue(expectedTotal.ToString("C"));
    }
    
    [TestMethod]
    public async Task Can_Navigate_Between_Catalog_And_Cart()
    {
        // Arrange
        var productCatalog = _fluentUI.NavigateTo<ProductCatalogPage>();
        
        // Act - Add items and navigate to cart
        productCatalog.AddPremiumWidget().AddStandardWidget();
        var shoppingCart = _fluentUI.NavigateTo<ShoppingCartPage>();
        
        // Assert - Verify cart has items
        shoppingCart.VerifyItemCount(2);
        
        // Act - Navigate back to catalog
        var backToCatalog = _fluentUI.NavigateTo<ProductCatalogPage>();
        
        // Assert - Verify cart state persisted
        backToCatalog.VerifyCartItemCount(2);
    }
    
    [TestMethod]
    public async Task Can_Handle_Empty_Cart()
    {
        // Arrange
        var shoppingCart = _fluentUI.NavigateTo<ShoppingCartPage>();
        
        // Assert
        shoppingCart
            .Verify(e => e.EmptyCartMessage).IsVisible().HasText("Your cart is empty")
            .Verify(e => e.CartSummary).IsVisible().HasClass("hidden");
    }
}
```

### 3. Documentation Updates

Update the documentation to include Example 2:

#### API Documentation
```markdown
# Example 2: Shopping Cart with Dynamic Pricing

This example demonstrates advanced verification patterns and state management in FluentUIScaffold V2.0, including:

- Internal fluent verification API
- Cross-page state management
- Dynamic pricing calculations
- Complex verification scenarios

## Complete Example

```csharp
fluentUI
    .NavigateTo<ProductCatalogPage>()
    .Click(e => e.PremiumWidgetAddButton)
    .Click(e => e.PremiumWidgetAddButton)
    .Click(e => e.StandardWidgetAddButton)
    .NavigateTo<ShoppingCartPage>()
    .Verify(e => e.CartItemCount, "3")
    .Verify(e => e.SubTotalElement, "$299.99")
    .Verify(e => e.TaxAmount, "$24.00")
    .Verify(e => e.ShippingCost.HasText("$15.99"))
    .Verify(e => e.TotalAmount.IsVisible().HasValue("$339.98"));
```

## Key Features Demonstrated

1. **Internal Fluent Verification**: Chained verification methods like `HasText()` and `IsVisible()`
2. **State Management**: Cart state persists across page navigation
3. **Dynamic Calculations**: Real-time pricing calculations with tax and shipping
4. **Complex Verification**: Multiple verification patterns in a single flow
5. **Cross-Page Integration**: Seamless navigation between catalog and cart
```

### 4. Sample App Integration

Ensure the sample app is fully integrated with Example 2:

#### Navigation Updates
Update the navigation to include shopping cart links:

```html
<nav>
    <a href="/">Home</a>
    <a href="/catalog">Products</a>
    <a href="/cart">Cart</a>
    <a href="/register">Register</a>
    <a href="/login">Login</a>
</nav>
```

#### Home Page Updates
Update the home page to include shopping cart features:

```html
<div class="featured-section">
    <h2>Featured Products</h2>
    <div class="product-preview">
        <h3>Premium Widget</h3>
        <p>High-quality widget with advanced features</p>
        <a href="/catalog" class="btn btn-primary">Shop Now</a>
    </div>
    <div class="cart-preview">
        <h3>Shopping Cart</h3>
        <p>Manage your items and view pricing</p>
        <a href="/cart" class="btn btn-secondary">View Cart</a>
    </div>
</div>
```

## Implementation Tasks

### Phase 1: Complete Implementation
1. [ ] Integrate all components from previous stories
2. [ ] Create complete working example
3. [ ] Test all shopping cart functionality
4. [ ] Verify all verification methods work correctly

### Phase 2: Comprehensive Testing
1. [ ] Create product catalog tests
2. [ ] Create shopping cart tests
3. [ ] Create integration tests
4. [ ] Test error scenarios and validation

### Phase 3: Documentation
1. [ ] Update API documentation with Example 2
2. [ ] Create tutorials and best practices
3. [ ] Add code examples and explanations
4. [ ] Update sample app documentation

### Phase 4: Sample App Integration
1. [ ] Add navigation between pages
2. [ ] Ensure all cart functionality works correctly
3. [ ] Test complete shopping flows
4. [ ] Verify all tests pass

## Dependencies

- **Story 2.1.1**: Implement Internal Fluent Verification API (must be completed first)
- **Story 2.1.2**: Create Shopping Cart Pages (must be completed first)
- **Story 2.2.1**: Implement Cross-Page State Management (must be completed first)

## Estimation

- **Time Estimate**: 2-3 weeks
- **Complexity**: Medium
- **Risk**: Low (building on established foundation)

## Definition of Done

- [ ] Complete Example 2 scenario is implemented and working
- [ ] Comprehensive test suite is created and passing
- [ ] Documentation is updated with Example 2
- [ ] Sample app is fully integrated
- [ ] All shopping cart functionality works correctly
- [ ] All verification methods work correctly
- [ ] State management works across pages
- [ ] All acceptance criteria are met

## Notes

- This story represents the completion of Example 2
- All previous stories must be completed before this one
- The implementation should demonstrate advanced framework capabilities
- The test suite should be comprehensive and cover all scenarios
- Documentation should be clear and helpful for developers 