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

    public class ContinueExceptionEventArgs<T> : ExceptionEventArgs<T> where T: Exception
    {
        public ContinueExceptionEventArgs(T exception, bool ignoreAndContinue) : base(exception)
        {
            this.IgnoreAndContinue = ignoreAndContinue;
        }

        /// <summary>
        ///  Specifies whether operation in progress should ignore the exception and just continue.
        /// </summary>
        public bool IgnoreAndContinue
        {
            get;
            set;
        }
    }
}
