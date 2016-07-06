namespace Firebase.Database.Offline
{
    using System;

    /// <summary>
    /// Event args holding the <see cref="Exception"/> object.
    /// </summary>
    public class ExceptionEventArgs : EventArgs
    {
        public readonly Exception Exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="exception"> The exception. </param>
        public ExceptionEventArgs(Exception exception)
        {
            this.Exception = exception;
        }
    }
}
