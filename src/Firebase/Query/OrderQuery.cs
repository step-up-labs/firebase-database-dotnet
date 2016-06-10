namespace Firebase.Database.Query
{
    /// <summary>
    /// Represents a firebase ordering query, e.g. "?OrderBy=Foo"
    /// </summary>
    public class OrderQuery : ParameterQuery
    {
        private readonly string propertyName;

        public OrderQuery(ChildQuery parent, string propertyName, FirebaseClient client)
            : base(parent, "orderBy", client)
        {
            this.propertyName = propertyName;
        }

        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            return $"\"{this.propertyName}\"";
        }
    }
}
