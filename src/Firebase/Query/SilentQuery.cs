namespace Firebase.Database.Query
{
    /// <summary>
    /// Appends print=silent to the url.
    /// </summary>
    public class SilentQuery : ParameterQuery
    {
        public SilentQuery(FirebaseQuery parent, FirebaseClient client) 
            : base(parent, () => "print", client)
        {
        }

        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            return "silent";
        }
    }
}
