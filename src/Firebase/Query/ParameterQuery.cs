namespace Firebase.Database.Query
{
    /// <summary>
    /// Represents a parameter in firebase query, e.g. "?data=foo"
    /// </summary>
    public abstract class ParameterQuery : FirebaseQuery
    {
        private readonly string parameter;
        private readonly string separator;

        protected ParameterQuery(FirebaseQuery parent, string parameter, FirebaseClient client)
            : base(parent, client)
        {
            this.parameter = parameter;
            this.separator = (this.Parent is ChildQuery) ? "?" : "&";
        }

        protected override string BuildUrlSegment(FirebaseQuery child)
        {
            return $"{this.separator}{this.parameter}={this.BuildUrlParameter(child)}";
        }

        protected abstract string BuildUrlParameter(FirebaseQuery child);
    }
}
