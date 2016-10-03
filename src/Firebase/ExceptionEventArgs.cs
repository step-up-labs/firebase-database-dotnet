namespace Firebase.Database
{
    using System;

    /// <summary>
    /// Event args holding the <see cref="Exception"/> object.
    /// </summary>
    public class ExceptionEventArgs<T> : EventArgs where T : Exception
    {
        public readonly T Exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="exception"> The exception. </param>
        public ExceptionEventArgs(T exception)
        {
            this.Exception = exception;
        }
    }

    public class ExceptionEventArgs : ExceptionEventArgs<Exception>
    {
        public ExceptionEventArgs(Exception exception) : base(exception)
        {
        }
    }
}
