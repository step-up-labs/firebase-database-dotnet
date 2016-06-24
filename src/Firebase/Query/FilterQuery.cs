namespace Firebase.Database.Query 
{
    /// <summary>
    /// Represents a firebase filtering query, e.g. "?LimitToLast=10".
    /// </summary>
    public class FilterQuery : ParameterQuery 
    {
        private readonly string value;
        private readonly double? doubleValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent. </param>
        /// <param name="filter"> The filter. </param>
        /// <param name="value"> The value for filter. </param>
        /// <param name="client"> The owning client. </param>  
        public FilterQuery(FirebaseQuery parent, string filter, string value, FirebaseClient client)
            : base(parent, filter, client)
        {
            this.value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent. </param>
        /// <param name="filter"> The filter. </param>
        /// <param name="value"> The value for filter. </param>
        /// <param name="client"> The owning client. </param>
        public FilterQuery(FirebaseQuery parent, string filter, double value, FirebaseClient client)
            : base(parent, filter, client)
        {
            this.doubleValue = value;
        }

        /// <summary>
        /// The build url parameter.
        /// </summary>
        /// <param name="child"> The child. </param> 
        /// <returns> Url parameter part of the resulting path. </returns> 
        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            if (this.value != null)
            {
                return $"\"{this.value}\"";
            }
            else if (this.doubleValue.HasValue)
            {
                return this.doubleValue.Value.ToString();
            }

            return string.Empty;
        }
    }
}
