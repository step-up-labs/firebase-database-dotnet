namespace Firebase.Database.Query
{
    using System;

    /// <summary>
    /// Represents an auth parameter in firebase query, e.g. "?auth=xyz"
    /// </summary>
    public class AuthQuery : ParameterQuery
    {
        private readonly string token;

        public AuthQuery(FirebaseQuery parent, string token) : base(parent, "auth")
        {
            this.token = token;
        }

        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            return this.token;
        }
    }
}
