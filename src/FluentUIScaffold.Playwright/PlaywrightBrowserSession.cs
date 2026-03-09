using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Interfaces;

using Microsoft.Playwright;

namespace FluentUIScaffold.Playwright
{
    /// <summary>
    /// An isolated browser session for a single test.
    /// Owns an IBrowserContext and IPage.
    /// </summary>
    public class PlaywrightBrowserSession : IBrowserSession
    {
        private readonly IBrowserContext _context;
        private readonly IPage _page;
        private readonly SessionServiceProvider _sessionProvider;
        private bool _isDisposed;

        public PlaywrightBrowserSession(
            IBrowserContext context,
            IPage page,
            IBrowser browser,
            IServiceProvider rootProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _page = page ?? throw new ArgumentNullException(nameof(page));
            _sessionProvider = new SessionServiceProvider(rootProvider, page, context, browser);
        }

        public IServiceProvider ServiceProvider => _sessionProvider;

        public async Task NavigateToUrlAsync(Uri url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            await _page.GotoAsync(url.ToString(), new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
            }).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            await _context.CloseAsync().ConfigureAwait(false);
        }
    }
}
