using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace FluentUIScaffold.Playwright
{
    /// <summary>
    /// Lightweight wrapper IServiceProvider that checks session-local services first,
    /// then falls back to the root provider. Implements IServiceProviderIsService
    /// so ActivatorUtilities.CreateInstance can resolve constructors correctly.
    /// </summary>
    internal class SessionServiceProvider : IServiceProvider, IServiceProviderIsService
    {
        private readonly IServiceProvider _root;
        private readonly Dictionary<Type, object> _sessionServices;

        public SessionServiceProvider(
            IServiceProvider root,
            IPage page,
            IBrowserContext context,
            IBrowser browser)
        {
            _root = root;
            _sessionServices = new Dictionary<Type, object>
            {
                [typeof(IPage)] = page,
                [typeof(IBrowserContext)] = context,
                [typeof(IBrowser)] = browser,
            };
        }

        public object GetService(Type serviceType)
        {
            return _sessionServices.TryGetValue(serviceType, out var service)
                ? service
                : _root?.GetService(serviceType);
        }

        public bool IsService(Type serviceType)
        {
            if (_sessionServices.ContainsKey(serviceType))
                return true;
            var rootIsService = _root?.GetService(typeof(IServiceProviderIsService)) as IServiceProviderIsService;
            return rootIsService?.IsService(serviceType) ?? false;
        }
    }
}
