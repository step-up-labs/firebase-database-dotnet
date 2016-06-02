namespace Firebase.Database.Query 
{
    /// <summary>
    /// Represents a firebase filtering query, e.g. "?LimitToLast=10"
    /// </summary>
    public class FilterQuery : ParameterQuery 
    {
        private readonly string value;
        private readonly double? doubleValue;

        public FilterQuery(FirebaseQuery parent, string filter, string value)
            : base(parent, filter)
        {
            this.value = value;
        }

        public FilterQuery(FirebaseQuery parent, string filter, double value)
            : base(parent, filter)
        {
            this.doubleValue = value;
        }

        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            if (this.value != null)
            {
                return $"\"{this.value.ToLower()}\"";
            }
            else if (this.doubleValue.HasValue)
            {
                return this.doubleValue.Value.ToString();
            }

            return string.Empty;
        }
    }
}
