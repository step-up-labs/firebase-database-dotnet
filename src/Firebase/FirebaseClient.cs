namespace Firebase
{
    using Firebase.Query;

    /// <summary>
    /// Firebase client which acts as an entry point to the online database.
    /// </summary>
    public class FirebaseClient
    {
        private readonly string baseUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirebaseClient"/> class.
        /// </summary>
        /// <param name="baseUrl"> The base url. </param>
        public FirebaseClient(string baseUrl)
        {
            this.baseUrl = baseUrl;

            if (!this.baseUrl.EndsWith("/"))
            {
                this.baseUrl += "/";
            }
        }
         
        /// <summary>
        /// Queries for a child of the data root.
        /// </summary>
        /// <param name="resourceName"> Name of the child. </param>
        /// <returns> <see cref="ChildQuery"/>. </returns>
        public ChildQuery Child(string resourceName)
        {
            return new ChildQuery(this.baseUrl + resourceName);
        }
    }
}
