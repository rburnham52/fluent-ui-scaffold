using System;
using System.Runtime.Serialization;

namespace FluentUIScaffold.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when configuration validation fails.
    /// </summary>
    public class FluentUIScaffoldValidationException : FluentUIScaffoldException
    {
        /// <summary>
        /// Initializes a new instance of the FluentUIScaffoldValidationException class.
        /// </summary>
        /// <param name="message">The error message</param>
        public FluentUIScaffoldValidationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the FluentUIScaffoldValidationException class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="property">The property that failed validation</param>
        public FluentUIScaffoldValidationException(string message, string property) : base(message)
        {
            Property = property;
        }

        /// <summary>
        /// Initializes a new instance of the FluentUIScaffoldValidationException class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public FluentUIScaffoldValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the FluentUIScaffoldValidationException class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="property">The property that failed validation</param>
        /// <param name="innerException">The inner exception</param>
        public FluentUIScaffoldValidationException(string message, string property, Exception innerException) : base(message, innerException)
        {
            Property = property;
        }

        protected FluentUIScaffoldValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public FluentUIScaffoldValidationException() : base("Validation error") { }

        /// <summary>
        /// Gets the property that failed validation.
        /// </summary>
        public string? Property { get; }
    }
}
