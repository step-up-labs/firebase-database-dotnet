namespace Firebase.Database.Http
{
    /// <summary>
    /// Represents data returned after a successful POST to firebase server.
    /// </summary>
    public class PostResult
    {
        /// <summary>
        /// Gets or sets the generated key after a successful post. 
        /// </summary>
        public string Name
        {
            get;
            set;
        }
    }
}
