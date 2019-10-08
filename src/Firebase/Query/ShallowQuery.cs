namespace Firebase.Database.Query
{
    /// <summary>
    /// Appends shallow=true to the url.
    /// </summary>
    public class ShallowQuery : ParameterQuery
    {
        public ShallowQuery(FirebaseQuery parent, FirebaseClient client) 
            : base(parent, () => "shallow", client)
        {
        }

        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            return "true";
        }
    }
}
