namespace Firebase.Database.Offline
{
    public enum StreamingOptions
    {
        /// <summary>
        /// No realtime streaming.
        /// </summary>
        None,

        /// <summary>
        /// Streaming of only new items - not the existing ones.
        /// </summary>
        LatestOnly,

        /// <summary>
        /// Streaming of all items. This will also pull all existing items on start, so be mindful about the number of items in your DB. 
        /// When used, consider not setting the <see cref="InitialPullStrategy"/> to <see cref="InitialPullStrategy.Everything"/> because you would pointlessly pull everything twice.
        /// </summary>
        Everything
    }
}
