namespace Firebase.Streaming
{
    /// <summary>
    /// Firebase event which hold <see cref="EventType"/> and the object affected by the event.
    /// </summary>
    /// <typeparam name="T"> Type of object affected by the event. </typeparam>
    public class FirebaseEvent<T> : FirebaseObject<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FirebaseEvent{T}"/> class.
        /// </summary>
        /// <param name="key"> The key of the object. </param>
        /// <param name="obj"> The object. </param>
        /// <param name="eventType"> The event type. </param>
        public FirebaseEvent(string key, T obj, FirebaseEventType eventType)
            : base(key, obj)
        {
            this.EventType = eventType;
        }

        /// <summary>
        /// Gets the event type.
        /// </summary>
        public FirebaseEventType EventType
        {
            get;
        }
    }
}
