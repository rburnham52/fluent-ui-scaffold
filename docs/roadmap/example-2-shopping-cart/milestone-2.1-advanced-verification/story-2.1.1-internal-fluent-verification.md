# Story 2.1.1: Implement Internal Fluent Verification API

## Overview

Implement the internal fluent verification API required for Example 2 (Shopping Cart with Dynamic Pricing), including chained verification methods like `HasText()`, `IsVisible()`, `HasValue()`, and `ContainsText()`.

## Background

Example 2 requires advanced verification patterns to check shopping cart calculations and dynamic pricing. The V2.0 specification shows internal fluent verification API:

```csharp
.Verify(e => e.ShippingCost.HasText("$15.99"))                    // Internal fluent API
.Verify(e => e.TotalAmount.IsVisible().HasValue("$339.98"))       // Chained verifications
.Verify(e => e.CheckoutButton.IsEnabled())                        // Single verification
.Verify(e => e.StatusMessage.ContainsText("Success").IsVisible()) // Multiple verifications
```

This story focuses on implementing the internal fluent verification API that allows chained verification methods.

## Acceptance Criteria

- [ ] Implement `HasText(string text)` verification method
- [ ] Implement `IsVisible()` verification method
- [ ] Implement `HasValue(string value)` verification method
- [ ] Implement `ContainsText(string text)` verification method
- [ ] Support chained verification methods
- [ ] Create working example with shopping cart verification

## Technical Requirements

### 1. Internal Fluent Verification API Implementation

Extend the `IVerificationContext` interface and `VerificationContext` class:

```csharp
public interface IVerificationContext
{
    IVerificationContext HasText(string expectedText);
    IVerificationContext HasValue(string expectedValue);
    IVerificationContext IsVisible();
    IVerificationContext IsEnabled();
    IVerificationContext ContainsText(string expectedText);
    IVerificationContext MatchesPattern(string pattern);
    IVerificationContext IsClickable();
    IVerificationContext IsSelected();
    IVerificationContext HasAttribute(string attributeName, string expectedValue);
    IVerificationContext HasClass(string className);
    IVerificationContext HasStyle(string propertyName, string expectedValue);
}

public class VerificationContext : IVerificationContext
{
    private readonly IUIDriver _driver;
    private readonly FluentUIScaffoldOptions _options;
    private readonly ILogger _logger;
    private readonly IElement _element;
    
    public VerificationContext(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger, IElement element)
    {
        _driver = driver;
        _options = options;
        _logger = logger;
        _element = element;
    }
    
    public IVerificationContext HasText(string expectedText)
    {
        var actualText = _driver.GetText(_element.Selector);
        if (actualText != expectedText)
        {
            throw new ElementValidationException($"Expected text '{expectedText}', but got '{actualText}'");
        }
        return this;
    }
    
    public IVerificationContext HasValue(string expectedValue)
    {
        var actualValue = _driver.GetAttribute(_element.Selector, "value");
        if (actualValue != expectedValue)
        {
            throw new ElementValidationException($"Expected value '{expectedValue}', but got '{actualValue}'");
        }
        return this;
    }
    
    public IVerificationContext IsVisible()
    {
        if (!_driver.IsVisible(_element.Selector))
        {
            throw new ElementValidationException($"Element '{_element.Selector}' is not visible");
        }
        return this;
    }
    
    public IVerificationContext IsEnabled()
    {
        if (!_driver.IsEnabled(_element.Selector))
        {
            throw new ElementValidationException($"Element '{_element.Selector}' is not enabled");
        }
        return this;
    }
    
    public IVerificationContext ContainsText(string expectedText)
    {
        var actualText = _driver.GetText(_element.Selector);
        if (!actualText.Contains(expectedText))
        {
            throw new ElementValidationException($"Expected text to contain '{expectedText}', but got '{actualText}'");
        }
        return this;
    }
    
    public IVerificationContext MatchesPattern(string pattern)
    {
        var actualText = _driver.GetText(_element.Selector);
        if (!Regex.IsMatch(actualText, pattern))
        {
            throw new ElementValidationException($"Expected text to match pattern '{pattern}', but got '{actualText}'");
        }
        return this;
    }
    
    public IVerificationContext IsClickable()
    {
        if (!_driver.IsEnabled(_element.Selector) || !_driver.IsVisible(_element.Selector))
        {
            throw new ElementValidationException($"Element '{_element.Selector}' is not clickable");
        }
        return this;
    }
    
    public IVerificationContext IsSelected()
    {
        var isSelected = _driver.GetAttribute(_element.Selector, "selected");
        if (isSelected != "true" && isSelected != "selected")
        {
            throw new ElementValidationException($"Element '{_element.Selector}' is not selected");
        }
        return this;
    }
    
    public IVerificationContext HasAttribute(string attributeName, string expectedValue)
    {
        var actualValue = _driver.GetAttribute(_element.Selector, attributeName);
        if (actualValue != expectedValue)
        {
            throw new ElementValidationException($"Expected attribute '{attributeName}' to be '{expectedValue}', but got '{actualValue}'");
        }
        return this;
    }
    
    public IVerificationContext HasClass(string className)
    {
        var classAttribute = _driver.GetAttribute(_element.Selector, "class");
        if (!classAttribute.Contains(className))
        {
            throw new ElementValidationException($"Expected element to have class '{className}', but got '{classAttribute}'");
        }
        return this;
    }
    
    public IVerificationContext HasStyle(string propertyName, string expectedValue)
    {
        var styleValue = _driver.GetStyle(_element.Selector, propertyName);
        if (styleValue != expectedValue)
        {
            throw new ElementValidationException($"Expected style '{propertyName}' to be '{expectedValue}', but got '{styleValue}'");
        }
        return this;
    }
}
```

### 2. Updated BasePageComponent Integration

Update the `BasePageComponent<TDriver, TPage>` to support internal fluent verification:

```csharp
public abstract class BasePageComponent<TDriver, TPage> : IPageComponent<TDriver, TPage>
    where TDriver : class, IUIDriver
    where TPage : class, IPageComponent<TDriver, TPage>
{
    // Verification access with internal fluent API
    public IVerificationContext Verify(Func<TPage, IElement> elementSelector)
    {
        var element = GetElementFromSelector(elementSelector);
        return new VerificationContext(Driver, Options, Logger, element);
    }
    
    // Convenience methods for common verifications
    public virtual TPage VerifyElementHasText(Func<TPage, IElement> elementSelector, string expectedText)
    {
        Verify(elementSelector).HasText(expectedText);
        return (TPage)this;
    }
    
    public virtual TPage VerifyElementIsVisible(Func<TPage, IElement> elementSelector)
    {
        Verify(elementSelector).IsVisible();
        return (TPage)this;
    }
    
    public virtual TPage VerifyElementIsEnabled(Func<TPage, IElement> elementSelector)
    {
        Verify(elementSelector).IsEnabled();
        return (TPage)this;
    }
    
    public virtual TPage VerifyElementContainsText(Func<TPage, IElement> elementSelector, string expectedText)
    {
        Verify(elementSelector).ContainsText(expectedText);
        return (TPage)this;
    }
}
```

### 3. Shopping Cart Page Implementation

Create the ProductCatalogPage and ShoppingCartPage with advanced verification:

```csharp
public class ProductCatalogPage : BasePageComponent<PlaywrightDriver, ProductCatalogPage>
{
    public IElement PremiumWidgetAddButton { get; private set; }
    public IElement StandardWidgetAddButton { get; private set; }
    public IElement ProductList { get; private set; }
    public IElement CartIcon { get; private set; }
    
    public ProductCatalogPage(IServiceProvider serviceProvider, Uri urlPattern) 
        : base(serviceProvider, urlPattern)
    {
    }
    
    protected override void ConfigureElements()
    {
        PremiumWidgetAddButton = new Element("#premium-widget-add", "Premium Widget Add Button", ElementType.Button);
        StandardWidgetAddButton = new Element("#standard-widget-add", "Standard Widget Add Button", ElementType.Button);
        ProductList = new Element("#product-list", "Product List", ElementType.Div);
        CartIcon = new Element("#cart-icon", "Cart Icon", ElementType.Div);
    }
    
    // Custom methods for product catalog interactions
    public ProductCatalogPage AddPremiumWidget()
    {
        return this.Click(e => e.PremiumWidgetAddButton);
    }
    
    public ProductCatalogPage AddStandardWidget()
    {
        return this.Click(e => e.StandardWidgetAddButton);
    }
    
    public ProductCatalogPage VerifyProductAdded(string productName)
    {
        return this.Verify(e => e.CartIcon).ContainsText(productName).IsVisible();
    }
}

public class ShoppingCartPage : BasePageComponent<PlaywrightDriver, ShoppingCartPage>
{
    public IElement CartItemCount { get; private set; }
    public IElement SubTotalElement { get; private set; }
    public IElement TaxAmount { get; private set; }
    public IElement ShippingCost { get; private set; }
    public IElement TotalAmount { get; private set; }
    public IElement CheckoutButton { get; private set; }
    public IElement CartItems { get; private set; }
    
    public ShoppingCartPage(IServiceProvider serviceProvider, Uri urlPattern) 
        : base(serviceProvider, urlPattern)
    {
    }
    
    protected override void ConfigureElements()
    {
        CartItemCount = new Element("#cart-item-count", "Cart Item Count", ElementType.Span);
        SubTotalElement = new Element("#subtotal", "Subtotal", ElementType.Span);
        TaxAmount = new Element("#tax-amount", "Tax Amount", ElementType.Span);
        ShippingCost = new Element("#shipping-cost", "Shipping Cost", ElementType.Span);
        TotalAmount = new Element("#total-amount", "Total Amount", ElementType.Span);
        CheckoutButton = new Element("#checkout-button", "Checkout Button", ElementType.Button);
        CartItems = new Element("#cart-items", "Cart Items", ElementType.Div);
    }
    
    // Custom verification methods for shopping cart
    public ShoppingCartPage VerifyCartCalculations()
    {
        return this
            .Verify(e => e.ShippingCost).HasText("$15.99")
            .Verify(e => e.TotalAmount).IsVisible().HasValue("$339.98");
    }
    
    public ShoppingCartPage VerifyItemCount(int expectedCount)
    {
        return this.Verify(e => e.CartItemCount, expectedCount.ToString());
    }
    
    public ShoppingCartPage VerifySubtotal(string expectedSubtotal)
    {
        return this.Verify(e => e.SubTotalElement, expectedSubtotal);
    }
    
    public ShoppingCartPage VerifyTaxAmount(string expectedTax)
    {
        return this.Verify(e => e.TaxAmount, expectedTax);
    }
}
```

### 4. Working Example

Create a working example that demonstrates the internal fluent verification API:

```csharp
[TestMethod]
public async Task Can_Verify_Shopping_Cart_Calculations()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web(options =>
    {
        options.BaseUrl = new Uri("https://your-app.com");
    });
    
    // Act
    var productCatalog = fluentUI.NavigateTo<ProductCatalogPage>();
    
    productCatalog
        .AddPremiumWidget()
        .AddPremiumWidget()
        .AddStandardWidget();
    
    var shoppingCart = fluentUI.NavigateTo<ShoppingCartPage>();
    
    // Assert - Using internal fluent verification API
    shoppingCart
        .Verify(e => e.CartItemCount, "3")
        .Verify(e => e.SubTotalElement, "$299.99")
        .Verify(e => e.TaxAmount, "$24.00")
        .Verify(e => e.ShippingCost).HasText("$15.99")
        .Verify(e => e.TotalAmount).IsVisible().HasValue("$339.98")
        .Verify(e => e.CheckoutButton).IsEnabled().IsVisible();
}
```

## Implementation Tasks

### Phase 1: Core Verification API
1. [ ] Extend `IVerificationContext` interface with new methods
2. [ ] Implement all verification methods in `VerificationContext`
3. [ ] Add chaining support for fluent API
4. [ ] Add error handling and descriptive messages

### Phase 2: BasePageComponent Integration
1. [ ] Update `BasePageComponent` to support internal fluent verification
2. [ ] Add convenience methods for common verifications
3. [ ] Test integration with existing verification methods
4. [ ] Ensure type safety throughout method chains

### Phase 3: Shopping Cart Pages
1. [ ] Create ProductCatalogPage with element configuration
2. [ ] Create ShoppingCartPage with element configuration
3. [ ] Add custom verification methods for shopping cart
4. [ ] Test all verification scenarios

### Phase 4: Testing and Examples
1. [ ] Create comprehensive tests for internal fluent verification
2. [ ] Test chained verification methods
3. [ ] Test error scenarios and validation
4. [ ] Create working examples for all verification patterns

## Dependencies

- **Story 1.3.2**: Complete Example 1 Implementation (must be completed first)

## Estimation

- **Time Estimate**: 3-4 weeks
- **Complexity**: Medium
- **Risk**: Low (building on established foundation)

## Definition of Done

- [ ] Internal fluent verification API is implemented and working
- [ ] All verification methods support chaining
- [ ] ProductCatalogPage and ShoppingCartPage are created
- [ ] Working examples demonstrate all verification patterns
- [ ] Comprehensive tests are passing
- [ ] Error scenarios are properly handled
- [ ] All acceptance criteria are met

## Notes

- Internal fluent verification should be framework-agnostic
- The fluent API should maintain type safety throughout method chains
- Error messages should be descriptive and helpful
- Verification should support both simple and complex scenarios
- Chaining should be intuitive and readable 