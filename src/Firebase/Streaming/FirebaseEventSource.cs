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
        /// Event comes from online source fetched during initial pull (valid only for RealtimeDatabase).
        /// </summary>
        OnlineInitial,

        /// <summary>
        /// Event comes from online source received thru active stream.
        /// </summary>
        OnlineStream,

        /// <summary>
        /// Event comes from online source being fetched manually.
        /// </summary>
        OnlinePull,

        /// <summary>
        /// Event raised after successful online push (valid only for RealtimeDatabase which isn't streaming).
        /// </summary>
        OnlinePush,

        /// <summary>
        /// Event comes from an online source.
        /// </summary>
        Online = OnlineInitial | OnlinePull | OnlinePush | OnlineStream
    }
}
