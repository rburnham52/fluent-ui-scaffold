using System;
using System.Runtime.Serialization;

namespace FluentUIScaffold.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when plugin operations fail.
    /// </summary>
    public class FluentUIScaffoldPluginException : FluentUIScaffoldException
    {
        /// <summary>
        /// Initializes a new instance of the FluentUIScaffoldPluginException class.
        /// </summary>
        /// <param name="message">The error message</param>
        public FluentUIScaffoldPluginException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the FluentUIScaffoldPluginException class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public FluentUIScaffoldPluginException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FluentUIScaffoldPluginException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public FluentUIScaffoldPluginException() : base("Plugin error") { }
    }
}
