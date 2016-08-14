namespace Firebase.Database.Offline
{
    /// <summary>
    /// Specifies the strategy for initial pull of server data.
    /// </summary>
    public enum InitialPullStrategy
    {
        /// <summary>
        /// Don't pull anything.
        /// </summary>
        None, 

        /// <summary>
        /// Pull only what isn't already stored offline.
        /// </summary>
        MissingOnly,

        /// <summary>
        /// Pull everything that exists on the server.
        /// </summary>
        Everything,
    }
}
