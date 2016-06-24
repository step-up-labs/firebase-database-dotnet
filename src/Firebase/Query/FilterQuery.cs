namespace Firebase.Database.Query 
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Represents a firebase filtering query, e.g. "?LimitToLast=10".
    /// </summary>
    public class FilterQuery : ParameterQuery 
    {
        private readonly Func<string> valueFactory;
        private readonly Func<double> doubleValueFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent. </param>
        /// <param name="filterFactory"> The filter. </param>
        /// <param name="valueFactory"> The value for filter. </param>
        /// <param name="client"> The owning client. </param>  
        public FilterQuery(FirebaseQuery parent, Func<string> filterFactory, Func<string> valueFactory, FirebaseClient client)
            : base(parent, filterFactory, client)
        {
            this.valueFactory = valueFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent. </param>
        /// <param name="filterFactory"> The filter. </param>
        /// <param name="valueFactory"> The value for filter. </param>
        /// <param name="client"> The owning client. </param>
        public FilterQuery(FirebaseQuery parent, Func<string> filterFactory, Func<double> valueFactory, FirebaseClient client)
            : base(parent, filterFactory, client)
        {
            this.doubleValueFactory = valueFactory;
        }

        /// <summary>
        /// The build url parameter.
        /// </summary>
        /// <param name="child"> The child. </param> 
        /// <returns> Url parameter part of the resulting path. </returns> 
        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            if (this.valueFactory != null)
            {
                return $"\"{this.valueFactory()}\"";
            }
            else if (this.doubleValueFactory != null)
            {
                return this.doubleValueFactory().ToString(CultureInfo.InvariantCulture);
            }

            return string.Empty;
        }
    }
}
