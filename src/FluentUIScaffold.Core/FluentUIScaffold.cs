namespace FluentUIScaffold.Core
{
    /// <summary>
    /// Marker class for web applications.
    /// </summary>
    public sealed class WebApp
    {
        public static readonly WebApp Instance = new WebApp();
        private WebApp() { }
    }

    /// <summary>
    /// Marker class for mobile applications.
    /// </summary>
    public sealed class MobileApp
    {
        public static readonly MobileApp Instance = new MobileApp();
        private MobileApp() { }
    }
}
