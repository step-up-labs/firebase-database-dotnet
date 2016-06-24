namespace Firebase.Database.Query
{
    /// <summary>
    /// Represents a parameter in firebase query, e.g. "?data=foo".
    /// </summary>
    public abstract class ParameterQuery : FirebaseQuery
    {
        private readonly string parameter;
        private readonly string separator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent of this query. </param>
        /// <param name="parameter"> The parameter. </param>
        /// <param name="client"> The owning client. </param>
        protected ParameterQuery(FirebaseQuery parent, string parameter, FirebaseClient client)
            : base(parent, client)
        {
            this.parameter = parameter;
            this.separator = (this.Parent is ChildQuery) ? "?" : "&";
        }

        /// <summary>
        /// Build the url segment represented by this query. 
        /// </summary> 
        /// <param name="child"> The child. </param>
        /// <returns> The <see cref="string"/>. </returns>
        protected override string BuildUrlSegment(FirebaseQuery child)
        {
            return $"{this.separator}{this.parameter}={this.BuildUrlParameter(child)}";
        }

        /// <summary>
        /// The build url parameter.
        /// </summary>
        /// <param name="child"> The child. </param>
        /// <returns> The <see cref="string"/>. </returns>
        protected abstract string BuildUrlParameter(FirebaseQuery child);
    }
}
