using System;

namespace FluentUIScaffold.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a verification fails.
    /// </summary>
    public class VerificationException : FluentUIScaffoldException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public VerificationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public VerificationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
