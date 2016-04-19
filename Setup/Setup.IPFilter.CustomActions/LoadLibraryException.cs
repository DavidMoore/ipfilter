namespace IPFilter.Setup.CustomActions
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;

    /// <summary>
    /// Error when loading a library using <see cref="Win32Api.LoadLibraryEx"/>.
    /// </summary>
    public class LoadLibraryException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadLibraryException"/> class.
        /// </summary>
        /// <param name="file">The filename of the module being loaded.</param>
        /// <param name="lastWin32Error">The Win32 error code.</param>
        public LoadLibraryException(string file, int lastWin32Error) : base($"Error when loading module \"{file}\". Error code: {lastWin32Error}") {}

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadLibraryException"/> class.
        /// </summary>
        public LoadLibraryException() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadLibraryException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public LoadLibraryException(string message) : base(message) {}

        /// <summary>
        /// Formats the specified error message and parameters using <see cref="CultureInfo.CurrentCulture"/> then
        /// initializes a new instance of the <see cref="LoadLibraryException"/> class with the resulting message.
        /// </summary>
        /// <param name="message">The formattable error message to pass to <see cref="string.Format(System.IFormatProvider,string,object[])"/></param>
        /// <param name="args">Arguments to pass to <see cref="string.Format(System.IFormatProvider,string,object[])"/></param>
        public LoadLibraryException(string message, params object[] args) : base( string.Format(message, args)) {}

        /// <summary>
        /// Formats the specified error message and parameters using <see cref="CultureInfo.CurrentCulture"/> then
        /// initializes a new instance of the <see cref="LoadLibraryException"/> class with the resulting message.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        /// <param name="message">The formattable error message to pass to <see cref="string.Format(System.IFormatProvider,string,object[])"/></param>
        /// <param name="args">Arguments to pass to <see cref="string.Format(System.IFormatProvider,string,object[])"/></param>
        public LoadLibraryException(Exception innerException, string message, params object[] args) : base(string.Format(message, args), innerException) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadLibraryException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public LoadLibraryException(string message, Exception innerException) : base(message, innerException) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadLibraryException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null.</exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).</exception>
        protected LoadLibraryException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }
}