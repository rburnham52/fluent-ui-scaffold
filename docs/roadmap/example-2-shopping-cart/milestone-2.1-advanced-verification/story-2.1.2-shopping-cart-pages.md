# Story 2.1.2: Create Shopping Cart Pages

## Overview

Create the ProductCatalogPage and ShoppingCartPage with their corresponding sample app pages, implementing the complete shopping cart functionality for Example 2.

## Background

Example 2 requires a complete shopping cart system with product catalog and dynamic pricing calculations. The V2.0 specification shows:

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

This story focuses on creating the page classes and sample app implementation.

## Acceptance Criteria

- [ ] Create ProductCatalogPage with add to cart buttons
- [ ] Create ShoppingCartPage with pricing calculations
- [ ] Add sample app pages for product catalog and shopping cart
- [ ] Implement dynamic pricing calculations

## Technical Requirements

### 1. ProductCatalogPage Implementation

Create a complete ProductCatalogPage class:

```csharp
public class ProductCatalogPage : BasePageComponent<PlaywrightDriver, ProductCatalogPage>
{
    public IElement PremiumWidgetAddButton { get; private set; }
    public IElement StandardWidgetAddButton { get; private set; }
    public IElement BasicWidgetAddButton { get; private set; }
    public IElement ProductList { get; private set; }
    public IElement CartIcon { get; private set; }
    public IElement CartItemCount { get; private set; }
    public IElement SearchInput { get; private set; }
    public IElement FilterDropdown { get; private set; }
    
    public ProductCatalogPage(IServiceProvider serviceProvider, Uri urlPattern) 
        : base(serviceProvider, urlPattern)
    {
    }
    
    protected override void ConfigureElements()
    {
        PremiumWidgetAddButton = new Element("#premium-widget-add", "Premium Widget Add Button", ElementType.Button);
        StandardWidgetAddButton = new Element("#standard-widget-add", "Standard Widget Add Button", ElementType.Button);
        BasicWidgetAddButton = new Element("#basic-widget-add", "Basic Widget Add Button", ElementType.Button);
        ProductList = new Element("#product-list", "Product List", ElementType.Div);
        CartIcon = new Element("#cart-icon", "Cart Icon", ElementType.Div);
        CartItemCount = new Element("#cart-item-count", "Cart Item Count", ElementType.Span);
        SearchInput = new Element("#search-input", "Search Input", ElementType.Input);
        FilterDropdown = new Element("#filter-dropdown", "Filter Dropdown", ElementType.Select);
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
    
    public ProductCatalogPage AddBasicWidget()
    {
        return this.Click(e => e.BasicWidgetAddButton);
    }
    
    public ProductCatalogPage SearchProducts(string searchTerm)
    {
        return this.Type(e => e.SearchInput, searchTerm);
    }
    
    public ProductCatalogPage FilterByCategory(string category)
    {
        return this.Select(e => e.FilterDropdown, category);
    }
    
    public ProductCatalogPage VerifyCartItemCount(int expectedCount)
    {
        return this.Verify(e => e.CartItemCount, expectedCount.ToString());
    }
    
    public ProductCatalogPage VerifyProductAdded(string productName)
    {
        return this.Verify(e => e.CartIcon).ContainsText(productName).IsVisible();
    }
}
```

### 2. ShoppingCartPage Implementation

Create a complete ShoppingCartPage class:

```csharp
public class ShoppingCartPage : BasePageComponent<PlaywrightDriver, ShoppingCartPage>
{
    public IElement CartItemCount { get; private set; }
    public IElement SubTotalElement { get; private set; }
    public IElement TaxAmount { get; private set; }
    public IElement ShippingCost { get; private set; }
    public IElement TotalAmount { get; private set; }
    public IElement CheckoutButton { get; private set; }
    public IElement CartItems { get; private set; }
    public IElement EmptyCartMessage { get; private set; }
    public IElement ContinueShoppingButton { get; private set; }
    public IElement RemoveItemButtons { get; private set; }
    public IElement QuantityInputs { get; private set; }
    
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
        EmptyCartMessage = new Element("#empty-cart-message", "Empty Cart Message", ElementType.Div);
        ContinueShoppingButton = new Element("#continue-shopping", "Continue Shopping Button", ElementType.Button);
        RemoveItemButtons = new Element(".remove-item", "Remove Item Buttons", ElementType.Button);
        QuantityInputs = new Element(".quantity-input", "Quantity Inputs", ElementType.Input);
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
    
    public ShoppingCartPage VerifyCartIsEmpty()
    {
        return this.Verify(e => e.EmptyCartMessage).IsVisible().HasText("Your cart is empty");
    }
    
    public ShoppingCartPage RemoveItem(int itemIndex)
    {
        return this.Click(e => e.RemoveItemButtons);
    }
    
    public ShoppingCartPage UpdateQuantity(int itemIndex, int quantity)
    {
        return this.Type(e => e.QuantityInputs, quantity.ToString());
    }
    
    public ShoppingCartPage ContinueShopping()
    {
        return this.Click(e => e.ContinueShoppingButton);
    }
    
    public ShoppingCartPage ProceedToCheckout()
    {
        return this.Click(e => e.CheckoutButton);
    }
}
```

### 3. Sample App HTML Implementation

Create the HTML pages for the sample app:

#### Product Catalog Page (HTML)
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Product Catalog - FluentUIScaffold Sample</title>
    <style>
        .product-catalog {
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
        }
        .header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
        }
        .search-filter {
            display: flex;
            gap: 10px;
            margin-bottom: 20px;
        }
        .product-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
            gap: 20px;
        }
        .product-card {
            border: 1px solid #ddd;
            border-radius: 8px;
            padding: 20px;
            text-align: center;
        }
        .product-card h3 {
            margin: 0 0 10px 0;
        }
        .product-card .price {
            font-size: 1.5em;
            font-weight: bold;
            color: #007bff;
            margin: 10px 0;
        }
        .add-to-cart {
            background-color: #28a745;
            color: white;
            border: none;
            padding: 10px 20px;
            border-radius: 5px;
            cursor: pointer;
        }
        .add-to-cart:hover {
            background-color: #218838;
        }
        .cart-icon {
            position: relative;
            cursor: pointer;
        }
        .cart-count {
            position: absolute;
            top: -8px;
            right: -8px;
            background-color: #dc3545;
            color: white;
            border-radius: 50%;
            width: 20px;
            height: 20px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 12px;
        }
    </style>
</head>
<body>
    <div class="product-catalog">
        <div class="header">
            <h1>Product Catalog</h1>
            <div class="cart-icon" id="cart-icon">
                🛒 <span id="cart-item-count" class="cart-count">0</span>
            </div>
        </div>
        
        <div class="search-filter">
            <input type="text" id="search-input" placeholder="Search products...">
            <select id="filter-dropdown">
                <option value="">All Categories</option>
                <option value="premium">Premium</option>
                <option value="standard">Standard</option>
                <option value="basic">Basic</option>
            </select>
        </div>
        
        <div class="product-grid" id="product-list">
            <div class="product-card">
                <h3>Premium Widget</h3>
                <p>High-quality widget with advanced features</p>
                <div class="price">$149.99</div>
                <button id="premium-widget-add" class="add-to-cart">Add to Cart</button>
            </div>
            
            <div class="product-card">
                <h3>Standard Widget</h3>
                <p>Reliable widget for everyday use</p>
                <div class="price">$99.99</div>
                <button id="standard-widget-add" class="add-to-cart">Add to Cart</button>
            </div>
            
            <div class="product-card">
                <h3>Basic Widget</h3>
                <p>Simple widget for basic needs</p>
                <div class="price">$49.99</div>
                <button id="basic-widget-add" class="add-to-cart">Add to Cart</button>
            </div>
        </div>
    </div>

    <script>
        let cartItems = [];
        let cartTotal = 0;
        
        function updateCartCount() {
            document.getElementById('cart-item-count').textContent = cartItems.length;
        }
        
        function addToCart(productName, price) {
            cartItems.push({ name: productName, price: price });
            cartTotal += price;
            updateCartCount();
            
            // Store cart data in localStorage for persistence
            localStorage.setItem('cartItems', JSON.stringify(cartItems));
            localStorage.setItem('cartTotal', cartTotal);
        }
        
        // Add event listeners
        document.getElementById('premium-widget-add').addEventListener('click', () => {
            addToCart('Premium Widget', 149.99);
        });
        
        document.getElementById('standard-widget-add').addEventListener('click', () => {
            addToCart('Standard Widget', 99.99);
        });
        
        document.getElementById('basic-widget-add').addEventListener('click', () => {
            addToCart('Basic Widget', 49.99);
        });
        
        // Load cart data on page load
        window.addEventListener('load', () => {
            const savedCartItems = localStorage.getItem('cartItems');
            const savedCartTotal = localStorage.getItem('cartTotal');
            
            if (savedCartItems) {
                cartItems = JSON.parse(savedCartItems);
                cartTotal = parseFloat(savedCartTotal);
                updateCartCount();
            }
        });
        
        // Search functionality
        document.getElementById('search-input').addEventListener('input', (e) => {
            const searchTerm = e.target.value.toLowerCase();
            const productCards = document.querySelectorAll('.product-card');
            
            productCards.forEach(card => {
                const productName = card.querySelector('h3').textContent.toLowerCase();
                if (productName.includes(searchTerm)) {
                    card.style.display = 'block';
                } else {
                    card.style.display = 'none';
                }
            });
        });
        
        // Filter functionality
        document.getElementById('filter-dropdown').addEventListener('change', (e) => {
            const filterValue = e.target.value.toLowerCase();
            const productCards = document.querySelectorAll('.product-card');
            
            productCards.forEach(card => {
                const productName = card.querySelector('h3').textContent.toLowerCase();
                if (!filterValue || productName.includes(filterValue)) {
                    card.style.display = 'block';
                } else {
                    card.style.display = 'none';
                }
            });
        });
    </script>
</body>
</html>
```

#### Shopping Cart Page (HTML)
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Shopping Cart - FluentUIScaffold Sample</title>
    <style>
        .shopping-cart {
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
        }
        .cart-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
        }
        .cart-items {
            margin-bottom: 30px;
        }
        .cart-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 15px;
            border-bottom: 1px solid #eee;
        }
        .cart-item:last-child {
            border-bottom: none;
        }
        .item-details {
            flex: 1;
        }
        .item-quantity {
            display: flex;
            align-items: center;
            gap: 10px;
        }
        .quantity-input {
            width: 60px;
            padding: 5px;
            text-align: center;
        }
        .remove-item {
            background-color: #dc3545;
            color: white;
            border: none;
            padding: 5px 10px;
            border-radius: 3px;
            cursor: pointer;
        }
        .cart-summary {
            border-top: 2px solid #eee;
            padding-top: 20px;
        }
        .summary-row {
            display: flex;
            justify-content: space-between;
            margin-bottom: 10px;
        }
        .summary-row.total {
            font-size: 1.2em;
            font-weight: bold;
            border-top: 1px solid #eee;
            padding-top: 10px;
        }
        .cart-actions {
            display: flex;
            gap: 10px;
            margin-top: 20px;
        }
        .btn {
            padding: 10px 20px;
            border: none;
            border-radius: 5px;
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
        .empty-cart-message {
            text-align: center;
            padding: 40px;
            color: #6c757d;
        }
    </style>
</head>
<body>
    <div class="shopping-cart">
        <div class="cart-header">
            <h1>Shopping Cart</h1>
            <span id="cart-item-count">0 items</span>
        </div>
        
        <div id="cart-items" class="cart-items">
            <!-- Cart items will be dynamically populated -->
        </div>
        
        <div id="empty-cart-message" class="empty-cart-message" style="display: none;">
            Your cart is empty
        </div>
        
        <div id="cart-summary" class="cart-summary" style="display: none;">
            <div class="summary-row">
                <span>Subtotal:</span>
                <span id="subtotal">$0.00</span>
            </div>
            <div class="summary-row">
                <span>Tax (8%):</span>
                <span id="tax-amount">$0.00</span>
            </div>
            <div class="summary-row">
                <span>Shipping:</span>
                <span id="shipping-cost">$15.99</span>
            </div>
            <div class="summary-row total">
                <span>Total:</span>
                <span id="total-amount">$0.00</span>
            </div>
            
            <div class="cart-actions">
                <button id="continue-shopping" class="btn btn-secondary">Continue Shopping</button>
                <button id="checkout-button" class="btn btn-primary">Proceed to Checkout</button>
            </div>
        </div>
    </div>

    <script>
        let cartItems = [];
        let cartTotal = 0;
        
        function loadCartData() {
            const savedCartItems = localStorage.getItem('cartItems');
            const savedCartTotal = localStorage.getItem('cartTotal');
            
            if (savedCartItems) {
                cartItems = JSON.parse(savedCartItems);
                cartTotal = parseFloat(savedCartTotal);
                updateCartDisplay();
            }
        }
        
        function updateCartDisplay() {
            const cartItemsContainer = document.getElementById('cart-items');
            const emptyCartMessage = document.getElementById('empty-cart-message');
            const cartSummary = document.getElementById('cart-summary');
            const cartItemCount = document.getElementById('cart-item-count');
            
            if (cartItems.length === 0) {
                cartItemsContainer.style.display = 'none';
                emptyCartMessage.style.display = 'block';
                cartSummary.style.display = 'none';
                cartItemCount.textContent = '0 items';
            } else {
                cartItemsContainer.style.display = 'block';
                emptyCartMessage.style.display = 'none';
                cartSummary.style.display = 'block';
                cartItemCount.textContent = `${cartItems.length} items`;
                
                // Populate cart items
                cartItemsContainer.innerHTML = '';
                cartItems.forEach((item, index) => {
                    const itemElement = document.createElement('div');
                    itemElement.className = 'cart-item';
                    itemElement.innerHTML = `
                        <div class="item-details">
                            <h3>${item.name}</h3>
                            <p>$${item.price.toFixed(2)}</p>
                        </div>
                        <div class="item-quantity">
                            <input type="number" class="quantity-input" value="1" min="1" data-index="${index}">
                            <button class="remove-item" data-index="${index}">Remove</button>
                        </div>
                    `;
                    cartItemsContainer.appendChild(itemElement);
                });
                
                // Calculate totals
                const subtotal = cartItems.reduce((sum, item) => sum + item.price, 0);
                const tax = subtotal * 0.08;
                const shipping = 15.99;
                const total = subtotal + tax + shipping;
                
                document.getElementById('subtotal').textContent = `$${subtotal.toFixed(2)}`;
                document.getElementById('tax-amount').textContent = `$${tax.toFixed(2)}`;
                document.getElementById('shipping-cost').textContent = `$${shipping.toFixed(2)}`;
                document.getElementById('total-amount').textContent = `$${total.toFixed(2)}`;
            }
        }
        
        // Event listeners
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('remove-item')) {
                const index = parseInt(e.target.dataset.index);
                cartItems.splice(index, 1);
                cartTotal = cartItems.reduce((sum, item) => sum + item.price, 0);
                localStorage.setItem('cartItems', JSON.stringify(cartItems));
                localStorage.setItem('cartTotal', cartTotal);
                updateCartDisplay();
            }
        });
        
        document.getElementById('continue-shopping').addEventListener('click', () => {
            window.location.href = '/catalog';
        });
        
        document.getElementById('checkout-button').addEventListener('click', () => {
            alert('Proceeding to checkout...');
        });
        
        // Load cart data on page load
        window.addEventListener('load', loadCartData);
    </script>
</body>
</html>
```

### 4. Working Example

Create a working example that demonstrates the shopping cart functionality:

```csharp
[TestMethod]
public async Task Can_Complete_Shopping_Cart_Flow()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web(options =>
    {
        options.BaseUrl = new Uri("https://your-app.com");
    });
    
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
```

## Implementation Tasks

### Phase 1: Page Class Implementation
1. [ ] Create ProductCatalogPage with all elements
2. [ ] Create ShoppingCartPage with all elements
3. [ ] Add custom methods for cart interactions
4. [ ] Add verification methods for cart calculations

### Phase 2: Sample App HTML
1. [ ] Create product catalog page HTML with styling
2. [ ] Create shopping cart page HTML with styling
3. [ ] Add JavaScript for cart functionality
4. [ ] Add dynamic pricing calculations

### Phase 3: Integration
1. [ ] Add pages to sample app routing
2. [ ] Test cart functionality and calculations
3. [ ] Verify all elements are properly configured
4. [ ] Test cart persistence and state management

### Phase 4: Testing
1. [ ] Create comprehensive tests for shopping cart flow
2. [ ] Test dynamic pricing calculations
3. [ ] Test cart state management
4. [ ] Test error scenarios and validation

## Dependencies

- **Story 2.1.1**: Implement Internal Fluent Verification API (must be completed first)

## Estimation

- **Time Estimate**: 2-3 weeks
- **Complexity**: Medium
- **Risk**: Low (building on established foundation)

## Definition of Done

- [ ] ProductCatalogPage class is implemented and working
- [ ] ShoppingCartPage class is implemented and working
- [ ] Sample app has working product catalog and shopping cart pages
- [ ] Dynamic pricing calculations work correctly
- [ ] Cart state management works correctly
- [ ] Comprehensive tests are passing
- [ ] Working example demonstrates complete flow
- [ ] All acceptance criteria are met

## Notes

- The sample app should provide realistic cart functionality
- Dynamic pricing should be calculated correctly
- Cart state should persist across page navigation
- All cart interactions should work with the fluent API 