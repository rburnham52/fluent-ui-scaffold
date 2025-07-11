using System;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;

namespace FluentUIScaffold.Core.Drivers
{
    /// <summary>
    /// Default implementation of IUIDriver for demonstration and fallback purposes.
    /// </summary>
    public sealed class DefaultUIDriver : IUIDriver, IDisposable
    {
        public Uri CurrentUrl => new Uri("about:blank");
        public void Click(string selector) { }
        public void Type(string selector, string text) { }
        public void SelectOption(string selector, string value) { }
        public string GetText(string selector)
        {
            // Provide mock text for testing purposes
            if (selector == "h1")
                return "FluentUIScaffold Sample App";
            if (selector.Contains("button"))
                return "0";
            return string.Empty;
        }
        public bool IsVisible(string selector) => true;
        public bool IsEnabled(string selector) => true;
        public void WaitForElement(string selector) { }
        public void WaitForElementToBeVisible(string selector) { }
        public void WaitForElementToBeHidden(string selector) { }
        public void NavigateToUrl(Uri url) { }
        public void NavigateToUrl(string url) { }
        public TTarget NavigateTo<TTarget>() where TTarget : class => default!;
        public TDriver GetFrameworkDriver<TDriver>() where TDriver : class => default!;
        public static void Initialize() { }
        public void Dispose()
        {
            // No resources to dispose
            GC.SuppressFinalize(this);
        }
    }
}
