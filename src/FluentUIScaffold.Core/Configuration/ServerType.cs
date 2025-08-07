namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Defines the types of servers that can be launched.
    /// </summary>
    public enum ServerType
    {
        /// <summary>
        /// ASP.NET Core applications
        /// </summary>
        AspNetCore,

        /// <summary>
        /// Aspire App Host applications
        /// </summary>
        Aspire,

        /// <summary>
        /// Node.js applications
        /// </summary>
        NodeJs,

        /// <summary>
        /// ASP.NET Core applications launched in-process using WebApplicationFactory
        /// </summary>
        WebApplicationFactory
    }
}
