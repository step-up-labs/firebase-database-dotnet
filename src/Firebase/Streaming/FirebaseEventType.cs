namespace Firebase.Database.Streaming
{
    /// <summary>
    /// The type of event. 
    /// </summary>
    public enum FirebaseEventType
    {
        /// <summary>
        /// Item was inserted or updated.
        /// </summary>
        InsertOrUpdate,

        /// <summary>
        /// Item was deleted.
        /// </summary>
        Delete
    }
}
