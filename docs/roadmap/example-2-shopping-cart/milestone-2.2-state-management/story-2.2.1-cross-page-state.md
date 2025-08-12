# Story 2.2.1: Implement Cross-Page State Management

## Overview

Implement cross-page state management required for Example 2 (Shopping Cart with Dynamic Pricing), including cart state persistence, item counting, and pricing calculations that work across different pages.

## Background

Example 2 requires the ability to maintain cart state across page navigation. The shopping cart state must persist when users navigate between the product catalog and shopping cart pages. This story focuses on implementing state management that works seamlessly across pages.

## Acceptance Criteria

- [ ] Implement cart state persistence across pages
- [ ] Add cart item counting functionality
- [ ] Implement pricing calculations (subtotal, tax, shipping)
- [ ] Create working example with cart state management

## Technical Requirements

### 1. State Management System

Implement a state management system that persists data across page navigation:

```csharp
public interface IStateManager
{
    T GetState<T>(string key);
    void SetState<T>(string key, T value);
    bool HasState(string key);
    void ClearState(string key);
    void ClearAllState();
}

public class StateManager : IStateManager
{
    private readonly Dictionary<string, object> _state = new();
    private readonly ILogger _logger;
    
    public StateManager(ILogger logger)
    {
        _logger = logger;
    }
    
    public T GetState<T>(string key)
    {
        if (_state.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        return default(T);
    }
    
    public void SetState<T>(string key, T value)
    {
        _state[key] = value;
        _logger.LogDebug($"State set: {key} = {value}");
    }
    
    public bool HasState(string key)
    {
        return _state.ContainsKey(key);
    }
    
    public void ClearState(string key)
    {
        _state.Remove(key);
        _logger.LogDebug($"State cleared: {key}");
    }
    
    public void ClearAllState()
    {
        _state.Clear();
        _logger.LogDebug("All state cleared");
    }
}
```

### 2. Cart State Model

Create a cart state model to manage shopping cart data:

```csharp
public class CartItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string Category { get; set; }
    
    public decimal TotalPrice => Price * Quantity;
}

public class CartState
{
    public List<CartItem> Items { get; set; } = new();
    public decimal Subtotal => Items.Sum(item => item.TotalPrice);
    public decimal Tax => Subtotal * 0.08m; // 8% tax
    public decimal Shipping => Items.Any() ? 15.99m : 0m;
    public decimal Total => Subtotal + Tax + Shipping;
    public int ItemCount => Items.Sum(item => item.Quantity);
    
    public void AddItem(CartItem item)
    {
        var existingItem = Items.FirstOrDefault(i => i.Id == item.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            Items.Add(item);
        }
    }
    
    public void RemoveItem(string itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            Items.Remove(item);
        }
    }
    
    public void UpdateQuantity(string itemId, int quantity)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
        }
    }
    
    public void Clear()
    {
        Items.Clear();
    }
}
```

### 3. Updated Page Classes with State Management

Update the page classes to use state management:

```csharp
public class ProductCatalogPage : BasePageComponent<PlaywrightDriver, ProductCatalogPage>
{
    private readonly IStateManager _stateManager;
    
    public ProductCatalogPage(IServiceProvider serviceProvider, Uri urlPattern) 
        : base(serviceProvider, urlPattern)
    {
        _stateManager = serviceProvider.GetRequiredService<IStateManager>();
    }
    
    // ... existing element definitions ...
    
    // Custom methods for product catalog interactions with state management
    public ProductCatalogPage AddPremiumWidget()
    {
        var cartState = _stateManager.GetState<CartState>("Cart") ?? new CartState();
        
        cartState.AddItem(new CartItem
        {
            Id = "premium-widget",
            Name = "Premium Widget",
            Price = 149.99m,
            Quantity = 1,
            Category = "Premium"
        });
        
        _stateManager.SetState("Cart", cartState);
        
        return this.Click(e => e.PremiumWidgetAddButton);
    }
    
    public ProductCatalogPage AddStandardWidget()
    {
        var cartState = _stateManager.GetState<CartState>("Cart") ?? new CartState();
        
        cartState.AddItem(new CartItem
        {
            Id = "standard-widget",
            Name = "Standard Widget",
            Price = 99.99m,
            Quantity = 1,
            Category = "Standard"
        });
        
        _stateManager.SetState("Cart", cartState);
        
        return this.Click(e => e.StandardWidgetAddButton);
    }
    
    public ProductCatalogPage AddBasicWidget()
    {
        var cartState = _stateManager.GetState<CartState>("Cart") ?? new CartState();
        
        cartState.AddItem(new CartItem
        {
            Id = "basic-widget",
            Name = "Basic Widget",
            Price = 49.99m,
            Quantity = 1,
            Category = "Basic"
        });
        
        _stateManager.SetState("Cart", cartState);
        
        return this.Click(e => e.BasicWidgetAddButton);
    }
    
    public ProductCatalogPage VerifyCartItemCount(int expectedCount)
    {
        var cartState = _stateManager.GetState<CartState>("Cart");
        var actualCount = cartState?.ItemCount ?? 0;
        
        if (actualCount != expectedCount)
        {
            throw new ElementValidationException($"Expected cart item count {expectedCount}, but got {actualCount}");
        }
        
        return this.Verify(e => e.CartItemCount, expectedCount.ToString());
    }
}

public class ShoppingCartPage : BasePageComponent<PlaywrightDriver, ShoppingCartPage>
{
    private readonly IStateManager _stateManager;
    
    public ShoppingCartPage(IServiceProvider serviceProvider, Uri urlPattern) 
        : base(serviceProvider, urlPattern)
    {
        _stateManager = serviceProvider.GetRequiredService<IStateManager>();
    }
    
    // ... existing element definitions ...
    
    // Custom verification methods for shopping cart with state management
    public ShoppingCartPage VerifyCartCalculations()
    {
        var cartState = _stateManager.GetState<CartState>("Cart");
        if (cartState == null || !cartState.Items.Any())
        {
            throw new ElementValidationException("Cart is empty");
        }
        
        return this
            .Verify(e => e.SubTotalElement, cartState.Subtotal.ToString("C"))
            .Verify(e => e.TaxAmount, cartState.Tax.ToString("C"))
            .Verify(e => e.ShippingCost).HasText(cartState.Shipping.ToString("C"))
            .Verify(e => e.TotalAmount).IsVisible().HasValue(cartState.Total.ToString("C"));
    }
    
    public ShoppingCartPage VerifyItemCount(int expectedCount)
    {
        var cartState = _stateManager.GetState<CartState>("Cart");
        var actualCount = cartState?.ItemCount ?? 0;
        
        if (actualCount != expectedCount)
        {
            throw new ElementValidationException($"Expected cart item count {expectedCount}, but got {actualCount}");
        }
        
        return this.Verify(e => e.CartItemCount, expectedCount.ToString());
    }
    
    public ShoppingCartPage RemoveItem(string itemId)
    {
        var cartState = _stateManager.GetState<CartState>("Cart");
        if (cartState != null)
        {
            cartState.RemoveItem(itemId);
            _stateManager.SetState("Cart", cartState);
        }
        
        return this.Click(e => e.RemoveItemButtons);
    }
    
    public ShoppingCartPage UpdateQuantity(string itemId, int quantity)
    {
        var cartState = _stateManager.GetState<CartState>("Cart");
        if (cartState != null)
        {
            cartState.UpdateQuantity(itemId, quantity);
            _stateManager.SetState("Cart", cartState);
        }
        
        return this.Type(e => e.QuantityInputs, quantity.ToString());
    }
    
    public ShoppingCartPage ClearCart()
    {
        var cartState = _stateManager.GetState<CartState>("Cart");
        if (cartState != null)
        {
            cartState.Clear();
            _stateManager.SetState("Cart", cartState);
        }
        
        return this;
    }
}
```

### 4. Service Registration

Update the service registration to include state management:

```csharp
public static class FluentUIScaffoldBuilder
{
    private static void ConfigureServices(IServiceCollection services, FluentUIScaffoldOptions options, Action<FrameworkOptions> configureFramework)
    {
        // Register framework-specific services
        configureFramework?.Invoke(new FrameworkOptions(services));
        
        // Register state management
        services.AddSingleton<IStateManager, StateManager>();
        
        // Register pages with their URL patterns
        RegisterPages(services, options);
        
        // Register other services
        services.AddSingleton(options);
        services.AddLogging();
    }
    
    private static void RegisterPages(IServiceCollection services, FluentUIScaffoldOptions options)
    {
        // Register pages with their URL patterns
        services.AddTransient<ProductCatalogPage>(provider => 
            new ProductCatalogPage(provider, new Uri(options.BaseUrl, "/catalog")));
        services.AddTransient<ShoppingCartPage>(provider => 
            new ShoppingCartPage(provider, new Uri(options.BaseUrl, "/cart")));
        services.AddTransient<RegistrationPage>(provider => 
            new RegistrationPage(provider, new Uri(options.BaseUrl, "/register")));
        services.AddTransient<LoginPage>(provider => 
            new LoginPage(provider, new Uri(options.BaseUrl, "/login")));
        services.AddTransient<HomePage>(provider => 
            new HomePage(provider, new Uri(options.BaseUrl, "/")));
    }
}
```

### 5. Working Example

Create a working example that demonstrates cross-page state management:

```csharp
[TestMethod]
public async Task Can_Manage_Cart_State_Across_Pages()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web(options =>
    {
        options.BaseUrl = new Uri("https://your-app.com");
    });
    
    // Act - Add items to cart on product catalog page
    var productCatalog = fluentUI.NavigateTo<ProductCatalogPage>();
    
    productCatalog
        .AddPremiumWidget()
        .AddPremiumWidget()
        .AddStandardWidget()
        .VerifyCartItemCount(3);
    
    // Act - Navigate to shopping cart page
    var shoppingCart = fluentUI.NavigateTo<ShoppingCartPage>();
    
    // Assert - Verify cart state persisted across pages
    shoppingCart
        .VerifyItemCount(3)
        .Verify(e => e.SubTotalElement, "$299.99")
        .Verify(e => e.TaxAmount, "$24.00")
        .Verify(e => e.ShippingCost).HasText("$15.99")
        .Verify(e => e.TotalAmount).IsVisible().HasValue("$339.98");
    
    // Act - Remove an item
    shoppingCart.RemoveItem("premium-widget");
    
    // Act - Navigate back to product catalog
    var backToCatalog = fluentUI.NavigateTo<ProductCatalogPage>();
    
    // Assert - Verify cart state updated
    backToCatalog.VerifyCartItemCount(2);
}
```

## Implementation Tasks

### Phase 1: State Management System
1. [ ] Create `IStateManager` interface
2. [ ] Implement `StateManager` class
3. [ ] Add state persistence and retrieval
4. [ ] Add logging for state changes

### Phase 2: Cart State Model
1. [ ] Create `CartItem` class
2. [ ] Create `CartState` class with calculations
3. [ ] Add cart manipulation methods
4. [ ] Test cart state calculations

### Phase 3: Page Integration
1. [ ] Update ProductCatalogPage with state management
2. [ ] Update ShoppingCartPage with state management
3. [ ] Add state-aware verification methods
4. [ ] Test state persistence across pages

### Phase 4: Service Registration
1. [ ] Update service registration to include state management
2. [ ] Register state manager as singleton
3. [ ] Test dependency injection
4. [ ] Verify state management works correctly

## Dependencies

- **Story 2.1.1**: Implement Internal Fluent Verification API (must be completed first)
- **Story 2.1.2**: Create Shopping Cart Pages (must be completed first)

## Estimation

- **Time Estimate**: 2-3 weeks
- **Complexity**: Medium
- **Risk**: Low (building on established foundation)

## Definition of Done

- [ ] State management system is implemented and working
- [ ] Cart state model is implemented with calculations
- [ ] Page classes use state management correctly
- [ ] State persists across page navigation
- [ ] Cart calculations work correctly
- [ ] Comprehensive tests are passing
- [ ] Working example demonstrates state management
- [ ] All acceptance criteria are met

## Notes

- State management should be framework-agnostic
- Cart calculations should be accurate and consistent
- State should persist across page navigation
- Error handling should be robust
- Performance should be considered for large cart states 