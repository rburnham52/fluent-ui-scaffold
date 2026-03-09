using System;

namespace FluentUIScaffold.Core.Exceptions
{
    /// <summary>
    /// Thrown when attempting to enqueue actions on a page that has been frozen
    /// by a NavigateTo call. Once a page navigates to another page, it cannot
    /// accept new actions — use the target page instead.
    /// </summary>
    public class FrozenPageException : InvalidOperationException
    {
        public Type PageType { get; }

        public FrozenPageException(Type pageType)
            : base($"Cannot enqueue actions on Page<{pageType.Name}> because it has been frozen by a NavigateTo() call. " +
                   $"Use the target page returned by NavigateTo() to continue the chain.")
        {
            PageType = pageType;
        }

        public FrozenPageException(Type sourcePageType, Type targetPageType)
            : base($"Cannot call NavigateTo on Page<{sourcePageType.Name}> because it is already frozen " +
                   $"(previously navigated to {targetPageType.Name}). " +
                   $"Use the target page to continue the chain.")
        {
            PageType = sourcePageType;
        }
    }
}
