namespace Firebase.Database.Query
{
    /// <summary>
    /// Represents an auth parameter in firebase query, e.g. "?auth=xyz".
    /// </summary>
    public class AuthQuery : ParameterQuery
    {
        private readonly string token;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent.  </param>  
        /// <param name="token"> The authentication token. </param>
        /// <param name="client"> The owner. </param>
        public AuthQuery(FirebaseQuery parent, string token, FirebaseClient client) : base(parent, "auth", client)
        {
            this.token = token;
        }

        /// <summary>
        /// Build the url parameter value of this child. 
        /// </summary>
        /// <param name="child"> The child of this child. </param>
        /// <returns> The <see cref="string"/>. </returns>
        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            return this.token;
        }
    }
}
