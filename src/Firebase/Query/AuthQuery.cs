namespace Firebase.Database.Query
{
    /// <summary>
    /// Represents an auth parameter in firebase query, e.g. "?auth=xyz"
    /// </summary>
    public class AuthQuery : ParameterQuery
    {
        private readonly string token;

        public AuthQuery(FirebaseQuery parent, string token, FirebaseClient client) : base(parent, "auth", client)
        {
            this.token = token;
        }

        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            return this.token;
        }
    }
}
