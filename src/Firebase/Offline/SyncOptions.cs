namespace Firebase.Database.Offline
{
    /// <summary>
    /// Specifies type of sync requested for given data.
    /// </summary>
    public enum SyncOptions
    {
        /// <summary>
        /// No sync needed for given data. 
        /// </summary>
        None,

        /// <summary>
        /// Data should be pulled from firebase.
        /// </summary>
        Pull,

        /// <summary>
        /// Data should be pushed to firebase.
        /// </summary>
        Push
    }
}
