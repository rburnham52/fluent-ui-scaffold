using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Exceptions;

using Microsoft.Extensions.DependencyInjection;

namespace FluentUIScaffold.Core.Pages
{
    /// <summary>
    /// Base class for all page objects. Acts as a deferred execution chain builder.
    /// Page methods queue actions via <see cref="Enqueue"/> that execute sequentially when awaited.
    /// </summary>
    /// <typeparam name="TSelf">The concrete page type (CRTP pattern for fluent returns)</typeparam>
    public abstract class Page<TSelf> where TSelf : Page<TSelf>
    {
        private readonly List<Func<IServiceProvider, Task>> _actions;
        private readonly IServiceProvider _serviceProvider;
        private bool _isFrozen;
        private bool _isConsumed;

        /// <summary>
        /// Creates a new page with a fresh action list.
        /// Used by AppScaffold.NavigateTo for initial page creation.
        /// </summary>
        protected Page(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _actions = new List<Func<IServiceProvider, Task>>(capacity: 8);
        }

        /// <summary>
        /// Creates a new page sharing an existing action list.
        /// Used by NavigateTo to chain across page boundaries.
        /// </summary>
        internal Page(IServiceProvider serviceProvider, List<Func<IServiceProvider, Task>> sharedActions)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _actions = sharedActions ?? throw new ArgumentNullException(nameof(sharedActions));
        }

#if DEBUG
        /// <summary>
        /// Warns if a chain with queued actions is never awaited.
        /// Only active in DEBUG builds as a safety net.
        /// </summary>
        ~Page()
        {
            if (_actions.Count > 0 && !_isConsumed)
            {
                Trace.TraceWarning(
                    $"Page<{typeof(TSelf).Name}> chain with {_actions.Count} " +
                    $"actions was never awaited. Add 'await' before the chain.");
            }
        }
#endif

        /// <summary>
        /// Gets the service provider for this page (session provider with root fallback).
        /// </summary>
        protected IServiceProvider ServiceProvider => _serviceProvider;

        /// <summary>
        /// Makes this page awaitable. Executes all queued actions sequentially.
        /// </summary>
        public TaskAwaiter GetAwaiter()
        {
            // Do NOT ConfigureAwait the returned Task — let caller's
            // SynchronizationContext control the final continuation.
            return ExecuteAllAsync().GetAwaiter();
        }

        /// <summary>
        /// Enqueues a deferred action with no DI injection.
        /// The action executes when the chain is awaited.
        /// </summary>
        protected TSelf Enqueue(Func<Task> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            ThrowIfFrozen();

            _actions.Add(_ => action());
            return (TSelf)this;
        }

        /// <summary>
        /// Enqueues a deferred action that receives a service resolved from DI.
        /// The service is resolved at execution time, not enqueue time.
        /// </summary>
        protected TSelf Enqueue<T>(Func<T, Task> action) where T : notnull
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            ThrowIfFrozen();

            _actions.Add(sp =>
            {
                var service = sp.GetService(typeof(T))
                    ?? throw new InvalidOperationException(
                        $"Service of type '{typeof(T).Name}' could not be resolved from the session provider " +
                        $"while executing an action on Page<{typeof(TSelf).Name}>. " +
                        $"Ensure the service is registered or available in the browser session.");
                return action((T)service);
            });
            return (TSelf)this;
        }

        /// <summary>
        /// Switches to a different page, freezing this page and sharing the action list.
        /// The target page receives the same action list, so subsequent enqueues on the
        /// target append to the shared chain.
        /// </summary>
        public TTarget NavigateTo<TTarget>() where TTarget : Page<TTarget>
        {
            ThrowIfFrozen();
            _isFrozen = true;

            // Create the target page and share our action list.
            // Try the internal two-param constructor first (same assembly), then fall back
            // to the single-param constructor and inject the shared action list via reflection.
            TTarget targetPage;
            try
            {
                targetPage = (TTarget)(Activator.CreateInstance(
                    typeof(TTarget),
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
                    binder: null,
                    args: new object[] { _serviceProvider, _actions },
                    culture: null)
                    ?? throw new InvalidOperationException($"Failed to create instance of {typeof(TTarget).Name}."));
            }
            catch (MissingMethodException)
            {
                // External assembly pages won't have the two-param constructor.
                // Create with single-param, then replace the action list via reflection.
                targetPage = (TTarget)(Activator.CreateInstance(
                    typeof(TTarget),
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
                    binder: null,
                    args: new object[] { _serviceProvider },
                    culture: null)
                    ?? throw new InvalidOperationException($"Failed to create instance of {typeof(TTarget).Name}."));

                // Inject the shared action list so chains span across pages
                var actionsField = typeof(Page<TTarget>).GetField("_actions",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                actionsField!.SetValue(targetPage, _actions);
            }

            // Enqueue navigation to the target page's route
            var routeAttribute = typeof(TTarget).GetCustomAttributes(typeof(RouteAttribute), inherit: true);
            if (routeAttribute.Length > 0)
            {
                var route = ((RouteAttribute)routeAttribute[0]).Path;
                _actions.Add(async sp =>
                {
                    var options = sp.GetService(typeof(Configuration.FluentUIScaffoldOptions))
                        as Configuration.FluentUIScaffoldOptions;
                    if (options?.BaseUrl != null)
                    {
                        var baseUrlString = options.BaseUrl.ToString().TrimEnd('/');
                        var routePath = route.StartsWith("/") ? route : "/" + route;
                        var url = new Uri(baseUrlString + routePath);

                        // Resolve IBrowserSession-like navigation via the session provider
                        // This will be available once Phase 2/3 are implemented
                        var session = sp.GetService(typeof(Interfaces.IBrowserSession))
                            as Interfaces.IBrowserSession;
                        if (session != null)
                        {
                            await session.NavigateToUrlAsync(url).ConfigureAwait(false);
                        }
                    }
                });
            }

            return targetPage;
        }

        /// <summary>
        /// Returns this page typed as TSelf for fluent chaining.
        /// </summary>
        protected TSelf Self => (TSelf)this;

        private async Task ExecuteAllAsync()
        {
#if DEBUG
            GC.SuppressFinalize(this);
#endif
            if (_isConsumed) return;
            _isConsumed = true;

            // Take a snapshot and clear to release closure references for GC
            var snapshot = _actions.ToArray();
            _actions.Clear();

            for (int i = 0; i < snapshot.Length; i++)
            {
                await snapshot[i](_serviceProvider).ConfigureAwait(false);
            }
        }

        private void ThrowIfFrozen()
        {
            if (_isFrozen)
            {
                throw new FrozenPageException(typeof(TSelf));
            }
        }
    }
}
