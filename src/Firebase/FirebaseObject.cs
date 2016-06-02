namespace Firebase.Database
{
    /// <summary>
    /// Holds the object of type <typeparam name="T" /> along with its key. 
    /// </summary>
    /// <typeparam name="T"> Type of the underlying object. </typeparam> 
    public class FirebaseObject<T> 
    {
        internal FirebaseObject(string key, T obj)
        {
            this.Key = key;
            this.Object = obj;
        }

        /// <summary>
        /// Gets the key of <see cref="Object"/>.
        /// </summary>
        public string Key
        {
            get;
        }

        /// <summary>
        /// Gets the underlying object.
        /// </summary>
        public T Object
        {
            get;
        }
    }
}
