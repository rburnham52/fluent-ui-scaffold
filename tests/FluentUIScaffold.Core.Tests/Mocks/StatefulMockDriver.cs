using System;
using System.Collections.Generic;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;

namespace FluentUIScaffold.Core.Tests.Mocks
{
    /// <summary>
    /// Stateful mock driver that simulates time-based state transitions for testing polling logic.
    /// Unlike MockUIDriver which returns static values, this driver can simulate elements
    /// becoming visible, text appearing after delay, etc.
    /// </summary>
    public sealed class StatefulMockDriver : IUIDriver, IDisposable
    {
        private readonly Dictionary<string, int> _callCounts = new Dictionary<string, int>();
        private readonly Dictionary<string, Func<int, bool>> _visibilityRules = new Dictionary<string, Func<int, bool>>();
        private readonly Dictionary<string, Func<int, string>> _textRules = new Dictionary<string, Func<int, string>>();
        private readonly Dictionary<string, Func<int, string>> _attributeRules = new Dictionary<string, Func<int, string>>();
        private readonly Dictionary<string, bool> _currentVisibilityState = new Dictionary<string, bool>();
        private Func<int, Uri>? _urlRule;
        private Func<int, string>? _titleRule;
        private int _globalCallCount = 0;

        public Uri CurrentUrl => _urlRule?.Invoke(_globalCallCount++) ?? new Uri("about:blank");

        public void Click(string selector) { }
        public void Type(string selector, string text) { }
        public void SelectOption(string selector, string value) { }

        public string GetText(string selector)
        {
            if (_textRules.TryGetValue(selector, out var rule))
            {
                return rule(_globalCallCount++);
            }
            return string.Empty;
        }

        public string GetAttribute(string selector, string attributeName)
        {
            var key = $"{selector}:{attributeName}";
            if (_attributeRules.TryGetValue(key, out var rule))
            {
                return rule(_globalCallCount++);
            }
            return string.Empty;
        }

        public string GetValue(string selector)
        {
            return GetAttribute(selector, "value");
        }

        public bool IsVisible(string selector)
        {
            // Return cached state if we've already waited for this element
            if (_currentVisibilityState.TryGetValue(selector, out var cachedState))
            {
                return cachedState;
            }

            // Otherwise, check the rule with current state
            if (_visibilityRules.TryGetValue(selector, out var rule))
            {
                return rule(_globalCallCount);
            }
            return true; // Default to visible
        }

        public bool IsEnabled(string selector) => true;

        public void WaitForElement(string selector)
        {
            // For simplicity, treat this the same as WaitForElementToBeVisible
            WaitForElementToBeVisible(selector);
        }

        public void WaitForElementToBeVisible(string selector)
        {
            if (_visibilityRules.TryGetValue(selector, out var rule))
            {
                // Simulate polling until element becomes visible
                for (int pollCount = 0; pollCount < 100; pollCount++)
                {
                    _globalCallCount++;
                    if (rule(_globalCallCount))
                    {
                        _currentVisibilityState[selector] = true;
                        return;
                    }
                }
                throw new Exceptions.TimeoutException($"Element '{selector}' never became visible");
            }
            else
            {
                // No rule means element is visible
                _currentVisibilityState[selector] = true;
            }
        }

        public void WaitForElementToBeHidden(string selector)
        {
            if (_visibilityRules.TryGetValue(selector, out var rule))
            {
                // Simulate polling until element becomes hidden
                for (int pollCount = 0; pollCount < 100; pollCount++)
                {
                    _globalCallCount++;
                    if (!rule(_globalCallCount))
                    {
                        _currentVisibilityState[selector] = false;
                        return;
                    }
                }
                throw new Exceptions.TimeoutException($"Element '{selector}' never became hidden");
            }
            else
            {
                // No rule means element stays visible, so can't wait for hidden
                throw new Exceptions.TimeoutException($"Element '{selector}' never became hidden");
            }
        }

        public void Focus(string selector) { }
        public void Hover(string selector) { }
        public void Clear(string selector) { }

        public string GetPageTitle()
        {
            return _titleRule?.Invoke(_globalCallCount++) ?? "Default Title";
        }

        public void NavigateToUrl(Uri url) { }
        public TTarget NavigateTo<TTarget>() where TTarget : class => default!;
        public TDriver GetFrameworkDriver<TDriver>() where TDriver : class => default!;

        public void Dispose()
        {
            // No resources to dispose
        }

        // Configuration methods for test setup

        /// <summary>
        /// Configure when an element becomes visible based on call count.
        /// Example: SetVisibilityRule("button", callCount => callCount >= 3) means button is visible after 3rd call.
        /// </summary>
        public void SetVisibilityRule(string selector, Func<int, bool> rule)
        {
            _visibilityRules[selector] = rule;
        }

        /// <summary>
        /// Configure text that changes based on call count.
        /// Example: SetTextRule("h1", callCount => callCount >= 2 ? "Updated" : "Original")
        /// </summary>
        public void SetTextRule(string selector, Func<int, string> rule)
        {
            _textRules[selector] = rule;
        }

        /// <summary>
        /// Configure attribute value that changes based on call count.
        /// </summary>
        public void SetAttributeRule(string selector, string attributeName, Func<int, string> rule)
        {
            _attributeRules[$"{selector}:{attributeName}"] = rule;
        }

        /// <summary>
        /// Configure URL that changes based on call count.
        /// </summary>
        public void SetUrlRule(Func<int, Uri> rule)
        {
            _urlRule = rule;
        }

        /// <summary>
        /// Configure title that changes based on call count.
        /// </summary>
        public void SetTitleRule(Func<int, string> rule)
        {
            _titleRule = rule;
        }

        /// <summary>
        /// Reset all state and rules for a fresh test state.
        /// </summary>
        public void Reset()
        {
            _globalCallCount = 0;
            _callCounts.Clear();
            _visibilityRules.Clear();
            _textRules.Clear();
            _attributeRules.Clear();
            _currentVisibilityState.Clear();
            _urlRule = null;
            _titleRule = null;
        }
    }
}
