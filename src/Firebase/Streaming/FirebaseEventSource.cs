namespace Firebase.Database.Streaming
{
    /// <summary>
    /// Specifies the origin of given <see cref="FirebaseEvent{T}"/>
    /// </summary>
    public enum FirebaseEventSource
    {
        /// <summary>
        /// Event comes from an offline source.
        /// </summary>
        Offline,

        /// <summary>
        /// Event comes from an online source.
        /// </summary>
        Online
    }
}
