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
        /// Data should be put to firebase.
        /// </summary>
        Put,

        /// <summary>
        /// Data should be patched in firebase.
        /// </summary>
        Patch
    }
}
